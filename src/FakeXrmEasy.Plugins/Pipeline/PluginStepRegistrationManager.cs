using System;
using System.Linq;
using System.Reflection;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Middleware.Pipeline;
using FakeXrmEasy.Plugins.Definitions;
using FakeXrmEasy.Plugins.Pipeline.PipelineTypes;
using FakeXrmEasy.Plugins.PluginInstances;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions;
using FakeXrmEasy.Plugins.PluginSteps.PluginStepRegistrationFieldNames;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Pipeline
{
    /// <summary>
    /// Internal helper class that contains logic to register new plugin steps
    /// </summary>
    internal static class PluginStepRegistrationManager
    {
        internal static Guid RegisterPluginStepInternal<TPlugin>(IXrmFakedContext context,
            IPluginStepDefinition pluginStepDefinition)
            where TPlugin : IPlugin

        {
            return RegisterPluginStepInternal(context, typeof(TPlugin), pluginStepDefinition);
        }
        
        internal static Guid RegisterPluginStepInternal(IXrmFakedContext context,
                                                        Type pluginType,
                                                        IPluginStepDefinition pluginStepDefinition)

        {
            VerifyPluginStepDefinition(context, pluginStepDefinition);
            ValidatePluginStep(context, pluginStepDefinition);

            AddPipelineTypes(context);

            // Message and MessageFilter
            var sdkMessage = AddOrUseSdkMessage(context, pluginStepDefinition);
            var sdkMessageFilter = AddSdkMessageFilter(context, pluginStepDefinition);

            // Store Plugin Type as a record
            var assemblyName = pluginType.Assembly.GetName();
            var pluginTypeRecord = AddPluginType(context, pluginType, assemblyName);

            // Secure config
            var sdkMessageProcessingStepSecureConfig =
                AddSdkMessageProcessingStepSecureConfig(context, pluginStepDefinition);
            
            // Message Step
            var sdkMessageProcessingStepId = pluginStepDefinition.Id == Guid.Empty ? Guid.NewGuid() : pluginStepDefinition.Id;

            var sdkMessageProcessingStep = new Entity(PluginStepRegistrationEntityNames.SdkMessageProcessingStep)
            {
                Id = sdkMessageProcessingStepId,
                [SdkMessageProcessingStepFieldNames.EventHandler] = pluginTypeRecord.ToEntityReference(),
                [SdkMessageProcessingStepFieldNames.SdkMessageId] = sdkMessage.ToEntityReference(),
                [SdkMessageProcessingStepFieldNames.SdkMessageFilterId] = sdkMessageFilter?.ToEntityReference(),
                [SdkMessageProcessingStepFieldNames.FilteringAttributes] = pluginStepDefinition.FilteringAttributes != null ? string.Join(",", pluginStepDefinition.FilteringAttributes) : null,
                [SdkMessageProcessingStepFieldNames.Stage] = new OptionSetValue((int)pluginStepDefinition.Stage),
                [SdkMessageProcessingStepFieldNames.Mode] = new OptionSetValue((int)pluginStepDefinition.Mode),
                [SdkMessageProcessingStepFieldNames.Rank] = pluginStepDefinition.Rank,
                [SdkMessageProcessingStepFieldNames.Configuration] = pluginStepDefinition.Configurations?.UnsecureConfig,
                [SdkMessageProcessingStepFieldNames.SdkMessageProcessingStepSecureConfigId] = sdkMessageProcessingStepSecureConfig?.ToEntityReference()
            };
            context.AddEntityWithDefaults(sdkMessageProcessingStep);

            if (pluginStepDefinition.PluginInstance != null)
            {
                var pluginInstancesRepository = context.GetProperty<IPluginInstancesRepository>();
                pluginInstancesRepository.SetPluginInstance(sdkMessageProcessingStepId, pluginStepDefinition.PluginInstance);
            }
            
            AddSdkMessageProcessingStepImages(context, pluginStepDefinition, sdkMessageProcessingStep);

            return sdkMessageProcessingStepId;
        }
        
        internal static Entity AddPluginType(IXrmFakedContext context, Type pluginType, AssemblyName assemblyName)
        {
            var pluginTypeRecord = context
                .CreateQuery(PluginStepRegistrationEntityNames.PluginType)
                .FirstOrDefault(pt => string.Equals(pt.GetAttributeValue<string>(PluginTypeFieldNames.TypeName), pluginType.FullName)
                                      && string.Equals(pt.GetAttributeValue<string>(PluginTypeFieldNames.AssemblyName), assemblyName.Name));
            if (pluginTypeRecord == null)
            {
                pluginTypeRecord = new Entity(PluginStepRegistrationEntityNames.PluginType)
                {
                    Id = Guid.NewGuid(),
                    [PluginTypeFieldNames.Name] = pluginType.FullName,
                    [PluginTypeFieldNames.TypeName] = pluginType.FullName,
                    [PluginTypeFieldNames.AssemblyName] = assemblyName.Name,
                    [PluginTypeFieldNames.Major] = assemblyName.Version.Major,
                    [PluginTypeFieldNames.Minor] = assemblyName.Version.Minor,
                    [PluginTypeFieldNames.Version] = assemblyName.Version.ToString(),
                };
                context.AddEntityWithDefaults(pluginTypeRecord);
            }

            return pluginTypeRecord;
        }
        
        internal static void AddSdkMessageProcessingStepImages(IXrmFakedContext context,
            IPluginStepDefinition pluginStepDefinition,
            Entity sdkMessageProcessingStep)
        {
            //Images setup
            if (pluginStepDefinition.ImagesDefinitions != null)
            {
                foreach (var pluginImage in pluginStepDefinition.ImagesDefinitions)
                {
                    var sdkMessageProcessingStepImage = new Entity(PluginStepRegistrationEntityNames.SdkMessageProcessingStepImage)
                    {
                        Id = Guid.NewGuid(),
                        [SdkMessageProcessingStepImageFieldNames.Name] = pluginImage.Name,
                        [SdkMessageProcessingStepImageFieldNames.SdkMessageProcessingStepId] = sdkMessageProcessingStep.ToEntityReference(),
                        [SdkMessageProcessingStepImageFieldNames.ImageType] = new OptionSetValue((int)pluginImage.ImageType),
                        [SdkMessageProcessingStepImageFieldNames.Attributes] = pluginImage.Attributes != null ? string.Join(",", pluginImage.Attributes) : null,
                    };
                    context.AddEntityWithDefaults(sdkMessageProcessingStepImage);
                }
            }
        }
        
        internal static void ValidatePluginStep(IXrmFakedContext context,
            IPluginStepDefinition pluginStepDefinition)
        {
            if (context.HasProperty<PipelineOptions>())
            {
                var pipelineOptions = context.GetProperty<PipelineOptions>();
                if (pipelineOptions.UsePluginStepRegistrationValidation)
                {
                    var validator = context.GetProperty<IPluginStepValidator>();
                    bool isValid = validator.IsValid(pluginStepDefinition);
                    if (!isValid)
                    {
                        throw new InvalidPluginStepRegistrationException();
                    }
                }
            }

            if (pluginStepDefinition.Id != Guid.Empty &&
                context.ContainsEntity(PluginStepRegistrationEntityNames.SdkMessageProcessingStep, pluginStepDefinition.Id))
            {
                throw new PluginStepDefinitionAlreadyRegisteredException(pluginStepDefinition.Id);
            }
        }
        
        internal static Entity AddSdkMessageProcessingStepSecureConfig(IXrmFakedContext context,
            IPluginStepDefinition pluginStepDefinition)
        {
            Entity sdkMessageProcessingStepSecureConfig = null;
            if (pluginStepDefinition.Configurations != null)
            {
                var sdkMessageProcessingSecureConfigId = pluginStepDefinition.Configurations.SecureConfigId;
                sdkMessageProcessingStepSecureConfig = new Entity(PluginStepRegistrationEntityNames.SdkMessageProcessingStepSecureConfig)
                {
                    Id = sdkMessageProcessingSecureConfigId,
                    [SdkMessageProcessingStepSecureConfigFieldNames.SecureConfig] =
                        pluginStepDefinition.Configurations.SecureConfig
                };
                
                context.AddEntityWithDefaults(sdkMessageProcessingStepSecureConfig);
            }

            return sdkMessageProcessingStepSecureConfig;
        }
        
        internal static void AddPipelineTypes(IXrmFakedContext context)
        {
            if(context.ProxyTypesAssemblies.Count() > 0)
            {
                var hasSdkMessage = context.FindReflectedType(PluginStepRegistrationEntityNames.SdkMessage) != null;
                var hasSdkMessageFilter = context.FindReflectedType(PluginStepRegistrationEntityNames.SdkMessageFilter) != null;
                var hasSdkMessageProcessingStep = context.FindReflectedType(PluginStepRegistrationEntityNames.SdkMessageProcessingStep) != null;
                var hasPluginType = context.FindReflectedType(PluginStepRegistrationEntityNames.PluginType) != null;
                var hasSdkMessageProcessingStepSecureConfig = context.FindReflectedType(PluginStepRegistrationEntityNames.SdkMessageProcessingStepSecureConfig) != null;
                
                var hasAllPipelineTypes =
                    hasSdkMessage &&
                    hasSdkMessageFilter &&
                    hasSdkMessageProcessingStep &&
                    hasSdkMessageProcessingStepSecureConfig &&
                    hasPluginType;

                var hasSomePipelineTypes =
                    hasSdkMessage ||
                    hasSdkMessageFilter ||
                    hasSdkMessageProcessingStep ||
                    hasSdkMessageProcessingStepSecureConfig ||
                    hasPluginType;

                if(hasAllPipelineTypes)
                {
                    return;
                }

                if(hasSomePipelineTypes)
                {
                    throw new MissingPipelineTypesException();
                }

                context.EnableProxyTypes(Assembly.GetExecutingAssembly());
            }
        }
        
        internal static Entity AddOrUseSdkMessage(IXrmFakedContext context,
                                                        IPluginStepDefinition pluginStepDefinition)
        {
            var sdkMessage = context
                                .CreateQuery(PluginStepRegistrationEntityNames.SdkMessage)
                                .FirstOrDefault(sm => string.Equals(sm.GetAttributeValue<string>(SdkMessageFieldNames.Name), pluginStepDefinition.MessageName));

            if (sdkMessage == null)
            {
                sdkMessage = new Entity(PluginStepRegistrationEntityNames.SdkMessage)
                {
                    Id = Guid.NewGuid(),
                    [SdkMessageFieldNames.Name] = pluginStepDefinition.MessageName
                };
                context.AddEntityWithDefaults(sdkMessage);
            }

            return sdkMessage;
        }
                
        internal static Entity AddSdkMessageFilter(IXrmFakedContext context,
            IPluginStepDefinition pluginStepDefinition)
        {
            // Filter
            Entity sdkFilter = null;
            if (pluginStepDefinition.EntityTypeCode.HasValue || pluginStepDefinition.EntityLogicalName != null)
            {
                sdkFilter = new Entity(PluginStepRegistrationEntityNames.SdkMessageFilter)
                {
                    Id = Guid.NewGuid(),
                    [SdkMessageFilterFieldNames.EntityLogicalName] = pluginStepDefinition.EntityLogicalName,
                    [SdkMessageFilterFieldNames.PrimaryObjectTypeCode] = pluginStepDefinition.EntityTypeCode
                };
                context.AddEntityWithDefaults(sdkFilter);
            }
            return sdkFilter;
        }
        
        internal static void VerifyPluginStepDefinition(IXrmFakedContext context,
            IPluginStepDefinition pluginStepDefinition)
        {
            if (pluginStepDefinition.EntityTypeCode != null &&
                !string.IsNullOrWhiteSpace(pluginStepDefinition.EntityLogicalName))
            {
                throw new RegisterStepWithEntityTypeCodeAndEntityLogicalNameException(
                    pluginStepDefinition.EntityTypeCode.Value, pluginStepDefinition.EntityLogicalName);
            }

            if (pluginStepDefinition.EntityTypeCode != null)
            {
                if (!context.ProxyTypesAssemblies.Any())
                {
                    throw new RegisterEntityTypeCodePluginStepWithoutProxyTypesAssemblyException(pluginStepDefinition
                        .EntityTypeCode.Value);
                }
                
                var reflectedType = context.FindReflectedType(pluginStepDefinition.EntityTypeCode.Value);
                if (reflectedType == null)
                {
                    throw new RegisterInvalidEntityTypeCodePluginStepException(pluginStepDefinition
                        .EntityTypeCode.Value);
                }
                
                pluginStepDefinition.EntityLogicalName = (string)reflectedType.GetField("EntityLogicalName")?.GetValue(null);
            }
        }
    }
}