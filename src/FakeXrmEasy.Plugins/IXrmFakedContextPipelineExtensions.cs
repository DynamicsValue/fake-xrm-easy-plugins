using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Plugins;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using FakeXrmEasy.Plugins;
using FakeXrmEasy.Extensions;
using FakeXrmEasy.Plugins.PluginImages;
using FakeXrmEasy.Middleware.Pipeline;
using FakeXrmEasy.Plugins.Audit;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions;
using FakeXrmEasy.Plugins.PluginSteps.PluginStepRegistrationFieldNames;
using FakeXrmEasy.Plugins.PluginSteps.Extensions;
using FakeXrmEasy.Plugins.Definitions;
using FakeXrmEasy.Plugins.Pipeline.PipelineTypes;
using FakeXrmEasy.Plugins.PluginInstances;

namespace FakeXrmEasy.Pipeline
{
    /// <summary>
    /// Extension methods to register plugin steps
    /// </summary>
    public static class IXrmFakedContextPipelineExtensions
    {
        /// <summary>
        /// Registers a new plugin againts the specified plugin with the plugin step definition details provided
        /// When using this method the plugin class specified in the method signature will be used instead of the assembly and plugin
        /// types provided in the plugin step definition parameter.
        /// 
        /// All the other remaining settings in the plugin definition parameter will be used for registration.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="pluginStepDefinition">Info about the details of the plugin registration</param>
        /// <returns></returns>
        public static Guid RegisterPluginStep<TPlugin>(this IXrmFakedContext context,
                                            IPluginStepDefinition pluginStepDefinition)
            where TPlugin : IPlugin
        {
            return context.RegisterPluginStepInternal<TPlugin>(pluginStepDefinition);
        }

        /// <summary>
        /// Registers a plugin step using an entity logical name, ideal for late bound entities
        /// </summary>
        /// <typeparam name="TPlugin">The plugin assembly to register</typeparam>
        /// <param name="context">The IXrmFakedContext where the registration will be stored </param>
        /// <param name="entityLogicalName">The logical name of the entity</param>
        /// <param name="message">The message that will trigger the plugin (i.e. request name)</param>
        /// <param name="stage">The stage in which the plugin will trigger</param>
        /// <param name="mode">The execution mode (async or sync)</param>
        /// <param name="rank">If multiple plugins are registered for the same message, rank defines the order in which they'll be executed</param>
        /// <param name="filteringAttributes">Any filtering attributes (optional)</param>
        /// <param name="registeredImages">Any pre or post images (optional)</param>
        /// <returns></returns>
        [Obsolete("This method is deprecated, please start using the new RegisterPluginStep method that takes an IPluginStepDefinition as a parameter")]
        public static Guid RegisterPluginStep<TPlugin>(this IXrmFakedContext context,
                                                        string entityLogicalName,
                                                        string message,
                                                        ProcessingStepStage stage = ProcessingStepStage.Postoperation,
                                                        ProcessingStepMode mode = ProcessingStepMode.Synchronous,
                                                        int rank = 1,
                                                        string[] filteringAttributes = null,
                                                        IEnumerable<PluginImageDefinition> registeredImages = null)
            where TPlugin : IPlugin
        {
            var pluginStepDefinition = new PluginStepDefinition()
            {
                MessageName = message,
                Stage = stage,
                Mode = mode,
                Rank = rank,
                FilteringAttributes = filteringAttributes,
                EntityTypeCode = null,
                EntityLogicalName = entityLogicalName,
                ImagesDefinitions = registeredImages
            };

            return RegisterPluginStepInternal<TPlugin>(context, pluginStepDefinition);
        }


        /// <summary>
        /// Registers the <typeparamref name="TPlugin"/> as a SDK Message Processing Step for the Entity <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TPlugin">The plugin to register the step for.</typeparam>
        /// <typeparam name="TEntity">The entity to filter this step for.</typeparam>
        /// <param name="context">The faked context to register this plugin against</param>
        /// <param name="message">The message that should trigger the execution of plugin.</param>
        /// <param name="stage">The stage when the plugin should be executed.</param>
        /// <param name="mode">The mode in which the plugin should be executed.</param>
        /// <param name="rank">The order in which this plugin should be executed in comparison to other plugins registered with the same <paramref name="message"/> and <paramref name="stage"/>.</param>
        /// <param name="filteringAttributes">When not one of these attributes is present in the execution context, the execution of the plugin is prevented.</param>
        /// <param name="registeredImages">Optional, the any images to register against this plugin step</param>
        [Obsolete("This method is deprecated, please start using the new RegisterPluginStep method that takes an IPluginStepDefinition as a parameter")]
        public static Guid RegisterPluginStep<TPlugin, TEntity>(this IXrmFakedContext context,
                                                                    string message,
                                                                    ProcessingStepStage stage = ProcessingStepStage.Postoperation,
                                                                    ProcessingStepMode mode = ProcessingStepMode.Synchronous,
                                                                    int rank = 1,
                                                                    string[] filteringAttributes = null,
                                                                    IEnumerable<PluginImageDefinition> registeredImages = null)
            where TPlugin : IPlugin
            where TEntity : Entity, new()
        {
            var entity = new TEntity();
            var entityType = entity.GetType();
            if (entityType.IsSubclassOf(typeof(Entity)))
            {
                var pluginStepDefinition = new PluginStepDefinition()
                {
                    MessageName = message,
                    Stage = stage,
                    Mode = mode,
                    Rank = rank,
                    FilteringAttributes = filteringAttributes,
                    EntityLogicalName = entity.LogicalName,
                    ImagesDefinitions = registeredImages
                };

                return RegisterPluginStepInternal<TPlugin>(context, pluginStepDefinition);
            }
            else
            {
                throw new InvalidRegistrationMethodForLateBoundException();
            }
        }

        /// <summary>
        /// Registers the <typeparamref name="TPlugin"/> as a SDK Message Processing Step.
        /// </summary>
        /// <typeparam name="TPlugin">The plugin to register the step for.</typeparam>
        /// <param name="context">The faked context to register this plugin against</param>
        /// <param name="message">The message that should trigger the execution of plugin.</param>
        /// <param name="stage">The stage when the plugin should be executed.</param>
        /// <param name="mode">The mode in which the plugin should be executed.</param>
        /// <param name="rank">The order in which this plugin should be executed in comparison to other plugins registered with the same <paramref name="message"/> and <paramref name="stage"/>.</param>
        /// <param name="filteringAttributes">When not one of these attributes is present in the execution context, the execution of the plugin is prevented.</param>
        /// <param name="primaryEntityTypeCode">The entity type code to filter this step for.</param>
        /// <param name="registeredImages">Optional, the any images to register against this plugin step</param>
        [Obsolete("This method is deprecated, please start using the new RegisterPluginStep method that takes an IPluginStepDefinition as a parameter")]
        public static Guid RegisterPluginStep<TPlugin>(this IXrmFakedContext context,
                                                        string message,
                                                        ProcessingStepStage stage = ProcessingStepStage.Postoperation,
                                                        ProcessingStepMode mode = ProcessingStepMode.Synchronous,
                                                        int rank = 1,
                                                        string[] filteringAttributes = null,
                                                        int? primaryEntityTypeCode = null,
                                                        IEnumerable<PluginImageDefinition> registeredImages = null)
            where TPlugin : IPlugin
        {
            var pluginStepDefinition = new PluginStepDefinition()
            {
                MessageName = message,
                Stage = stage,
                Mode = mode,
                Rank = rank,
                FilteringAttributes = filteringAttributes,
                EntityTypeCode = primaryEntityTypeCode,
                EntityLogicalName = null,
                ImagesDefinitions = registeredImages
            };

            return RegisterPluginStepInternal<TPlugin>(context, pluginStepDefinition);
        }

        internal static Entity AddPluginType(this IXrmFakedContext context, Type pluginType, AssemblyName assemblyName)
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

        internal static Guid RegisterPluginStepInternal<TPlugin>(this IXrmFakedContext context,
                                                                IPluginStepDefinition pluginStepDefinition)
            where TPlugin : IPlugin

        {
            return context.RegisterPluginStepInternal(typeof(TPlugin), pluginStepDefinition);
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
        
        internal static Guid RegisterPluginStepInternal(this IXrmFakedContext context,
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
            var pluginTypeRecord = context.AddPluginType(pluginType, assemblyName);

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

        [Obsolete("This method is obsolete, should no longer be used as it has been replaced by a more efficient method 'GetPluginStepsForOrganizationRequest' that instead directly queries the FakeXrmEasy internal in-memory database")]
        internal static IEnumerable<PluginStepDefinition> GetPluginStepsForOrganizationRequestWithRetrieveMultiple(this IXrmFakedContext context, string requestName, ProcessingStepStage stage, ProcessingStepMode mode, OrganizationRequest request)
        {
            var target = GetTargetForRequest(request);
            if (target is Entity)
            {
                return context.GetStepsForStageWithRetrieveMultiple(requestName, stage, mode, target as Entity);
            }
            else if (target is EntityReference)
            {
                var entityReference = target as EntityReference;
                var entityType = context.FindReflectedType(entityReference.LogicalName);
                if (entityType == null)
                {
                    return null;
                }

                return context.GetStepsForStageWithRetrieveMultiple(requestName, stage, mode, (Entity)Activator.CreateInstance(entityType));
            }

            return null;
        }

        internal static IEnumerable<PluginStepDefinition> GetPluginStepsForOrganizationRequest(this IXrmFakedContext context, string requestName, ProcessingStepStage stage, ProcessingStepMode mode, OrganizationRequest request)
        {
            var target = GetTargetForRequest(request);
            if (target is Entity)
            {
                return context.GetStepsForStage(requestName, stage, mode, target as Entity);
            }
            else if (target is EntityReference)
            {
                var entityReference = target as EntityReference;
                var entityType = context.FindReflectedType(entityReference.LogicalName);
                if (entityType == null)
                {
                    return null;
                }

                return context.GetStepsForStage(requestName, stage, mode, (Entity)Activator.CreateInstance(entityType));
            }
            else
            {
                //Possibly a custom api execution
                return context.GetStepsForStage(requestName, stage, mode, null);
            }
        }

        internal static void ExecutePipelineStage(this IXrmFakedContext context, PipelineStageExecutionParameters parameters)
        {
            var plugins = context.GetPluginStepsForOrganizationRequest(parameters.RequestName, parameters.Stage, parameters.Mode, parameters.Request);
            if (plugins == null)
                return;

            var target = GetTargetForRequest(parameters.Request);

            if (target != null)
            {
                var entityTarget = target as Entity;
                if (entityTarget != null)
                {
                    context.ExecutePipelinePlugins(plugins, parameters.Request, parameters.PreEntitySnapshot, parameters.PostEntitySnapshot, parameters.Response);
                }

                var entityReferenceTarget = target as EntityReference;
                if (entityReferenceTarget != null)
                {
                    var entityType = context.FindReflectedType(entityReferenceTarget.LogicalName);
                    if (entityType == null)
                    {
                        return;
                    }
                    context.ExecutePipelinePlugins(plugins, parameters.Request, parameters.PreEntitySnapshot, parameters.PostEntitySnapshot, parameters.Response);
                }
            }
            else
            {
                context.ExecutePipelinePlugins(plugins, parameters.Request, null, null, parameters.Response);
            }
        }

        private static void ExecutePlugin(IXrmFakedContext context,
            PluginStepDefinition pluginStepDefinition,
            XrmFakedPluginExecutionContext pluginContext,
            bool isAuditEnabled)
        {
            var pluginMethod = GetPluginMethod(pluginStepDefinition);
            
            try
            {
                InvokePluginMethod(pluginMethod, context, pluginStepDefinition, pluginContext);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
            
            if (isAuditEnabled)
            {
                context.AddPluginStepAuditDetails(pluginMethod, pluginContext, pluginStepDefinition);
            }
        }
            
            
        /// <summary>
        /// Executes all the relevant plugin steps for the current request
        /// </summary>
        /// <param name="context"></param>
        /// <param name="pluginSteps"></param>
        /// <param name="organizationRequest"></param>
        /// <param name="previousValues"></param>
        /// <param name="resultingAttributes"></param>
        /// <param name="organizationResponse">The organization response that triggered this plugin execution</param>
        private static void ExecutePipelinePlugins(this IXrmFakedContext context,
                                                    IEnumerable<PluginStepDefinition> pluginSteps,
                                                    OrganizationRequest organizationRequest,
                                                    Entity previousValues,
                                                    Entity resultingAttributes,
                                                    OrganizationResponse organizationResponse)
        {
            var isAuditEnabled = context.GetProperty<PipelineOptions>().UsePluginStepAudit;

            foreach (var pluginStep in pluginSteps)
            {
                IEnumerable<Entity> preImageDefinitions = null;
                if (previousValues != null)
                {
                    preImageDefinitions = context.GetPluginImageDefinitions(pluginStep.Id, ProcessingStepImageType.PreImage);
                }

                IEnumerable<Entity> postImageDefinitions = null;
                if (resultingAttributes != null)
                {
                    postImageDefinitions = context.GetPluginImageDefinitions(pluginStep.Id, ProcessingStepImageType.PostImage);
                }

                var pluginContext = context.GetDefaultPluginContext();
                pluginContext.Mode = (int)pluginStep.Mode;
                pluginContext.Stage = (int)pluginStep.Stage;
                pluginContext.MessageName = pluginStep.MessageName;
                pluginContext.InputParameters = organizationRequest.Parameters;
                pluginContext.OutputParameters = organizationResponse != null ? organizationResponse.Results : new ParameterCollection();
                pluginContext.PreEntityImages = GetEntityImageCollection(preImageDefinitions, previousValues);
                pluginContext.PostEntityImages = GetEntityImageCollection(postImageDefinitions, resultingAttributes);

                ExecutePlugin(context, pluginStep, pluginContext, isAuditEnabled);
            }
        }

        private static void AddPluginStepAuditDetails(this IXrmFakedContext context,
                                MethodInfo pluginMethod,
                                XrmFakedPluginExecutionContext pluginContext,
                                PluginStepDefinition pluginStep)
        {
            Type pluginType = null;
            if (pluginStep.PluginInstance != null)
            {
                pluginType = pluginStep.PluginInstance.GetType();
            }
            else
            {
                pluginType = pluginMethod.GetGenericArguments()[0];
            }
            
            var pluginStepAuditDetails = new PluginStepAuditDetails()
            {
                PluginAssemblyType = pluginType,
                PluginStepId = pluginStep.Id,
                MessageName = pluginContext.MessageName,
                Stage = (ProcessingStepStage)pluginContext.Stage,
                InputParameters = pluginContext.InputParameters,
                OutputParameters = pluginContext.OutputParameters,
                PluginStepDefinition = pluginStep
            };

            if (pluginContext.InputParameters.ContainsKey("Target"))
            {
                var target = pluginContext.InputParameters["Target"];
                if (target is Entity)
                    pluginStepAuditDetails.TargetEntity = (Entity)target;

                if (target is EntityReference)
                    pluginStepAuditDetails.TargetEntityReference = (EntityReference)target;
            }

            var pluginStepAudit = context.GetProperty<IPluginStepAudit>() as PluginStepAudit;
            pluginStepAudit.Add(pluginStepAuditDetails);
        }

        private static MethodInfo GetPluginMethod(PluginStepDefinition pluginStepDefinition)
        {
            var assembly = AppDomain.CurrentDomain.Load(pluginStepDefinition.AssemblyName);
            var pluginType = assembly.GetType(pluginStepDefinition.PluginType);

            MethodInfo methodInfo = null;
            if (pluginStepDefinition.PluginInstance != null)
            {
                methodInfo = typeof(IXrmBaseContextPluginExtensions).GetMethod("ExecutePluginWith", new[] { typeof(IXrmFakedContext), typeof(XrmFakedPluginExecutionContext), typeof(IPlugin) });
                return methodInfo;
            }
            
            if (pluginStepDefinition.Configurations != null)
            {
                methodInfo = typeof(IXrmBaseContextPluginExtensions).GetMethod("ExecutePluginWithConfigurations", new[] { typeof(IXrmFakedContext), typeof(XrmFakedPluginExecutionContext), typeof(string), typeof(string) });
            }
            else
            {
                methodInfo = typeof(IXrmBaseContextPluginExtensions).GetMethod("ExecutePluginWith", new[] { typeof(IXrmFakedContext), typeof(XrmFakedPluginExecutionContext) });
            }
            
            return methodInfo.MakeGenericMethod(pluginType);
        }
        
        private static void InvokePluginMethod(MethodInfo pluginMethod, 
            IXrmFakedContext context,
            IPluginStepDefinition pluginStepDefinition, 
            XrmFakedPluginExecutionContext pluginContext)
        {
            if (pluginStepDefinition.PluginInstance != null)
            {
                pluginMethod.Invoke(null, new object[] { context, pluginContext, pluginStepDefinition.PluginInstance });
            }
            else if (pluginStepDefinition.Configurations != null)
            {
                pluginMethod.Invoke(null, new object[] { context, 
                    pluginContext, 
                    pluginStepDefinition.Configurations.UnsecureConfig,
                    pluginStepDefinition.Configurations.SecureConfig
                });
            }
            else
            {
                pluginMethod.Invoke(null, new object[] { context, pluginContext });
            }
        }

        private static IEnumerable<PluginStepDefinition> GetStepsForStageWithRetrieveMultiple(this IXrmFakedContext context, string requestName, ProcessingStepStage stage, ProcessingStepMode mode, Entity entity)
        {
            var query = new QueryExpression(PluginStepRegistrationEntityNames.SdkMessageProcessingStep)
            {
                ColumnSet = new ColumnSet(SdkMessageProcessingStepFieldNames.FilteringAttributes,
                                        SdkMessageProcessingStepFieldNames.Stage,
                                        SdkMessageProcessingStepFieldNames.Mode,
                                        SdkMessageProcessingStepFieldNames.Rank),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(SdkMessageProcessingStepFieldNames.Stage, ConditionOperator.Equal, (int)stage),
                        new ConditionExpression(SdkMessageProcessingStepFieldNames.Mode, ConditionOperator.Equal, (int)mode)
                    }
                },
                Orders =
                {
                    new OrderExpression(SdkMessageProcessingStepFieldNames.Rank, OrderType.Ascending)
                },
                LinkEntities =
                {
                    new LinkEntity(PluginStepRegistrationEntityNames.SdkMessageProcessingStep,
                                    PluginStepRegistrationEntityNames.SdkMessageFilter,
                                    SdkMessageProcessingStepFieldNames.SdkMessageFilterId,
                                    SdkMessageProcessingStepFieldNames.SdkMessageFilterId,
                                    JoinOperator.LeftOuter)
                    {
                        EntityAlias = PluginStepRegistrationEntityNames.SdkMessageFilter,
                        Columns = new ColumnSet(SdkMessageFilterFieldNames.PrimaryObjectTypeCode)
                    },
                    new LinkEntity(PluginStepRegistrationEntityNames.SdkMessageProcessingStep,
                                    PluginStepRegistrationEntityNames.SdkMessage,
                                    SdkMessageProcessingStepFieldNames.SdkMessageId,
                                    SdkMessageProcessingStepFieldNames.SdkMessageId,
                                    JoinOperator.Inner)
                    {
                        EntityAlias = PluginStepRegistrationEntityNames.SdkMessage,
                        Columns = new ColumnSet(SdkMessageFieldNames.Name),
                        LinkCriteria =
                        {
                            Conditions =
                            {
                                new ConditionExpression(SdkMessageFieldNames.Name, ConditionOperator.Equal, requestName)
                            }
                        }
                    },
                    new LinkEntity(PluginStepRegistrationEntityNames.SdkMessageProcessingStep,
                                    PluginStepRegistrationEntityNames.PluginType,
                                    SdkMessageProcessingStepFieldNames.EventHandler,
                                    PluginTypeFieldNames.PluginTypeId,
                                    JoinOperator.Inner)
                    {
                        EntityAlias = PluginStepRegistrationEntityNames.PluginType,
                        Columns = new ColumnSet(PluginTypeFieldNames.AssemblyName, PluginTypeFieldNames.TypeName)
                    }
                }
            };

            var entityTypeCode = (int?)entity.GetType().GetField("EntityTypeCode")?.GetValue(entity);
            var service = context.GetOrganizationService();
            var pluginSteps = service.RetrieveMultiple(query).Entities.AsEnumerable();
            pluginSteps = pluginSteps.Where(p =>
            {
                var primaryObjectTypeCode = p.GetAttributeValue<AliasedValue>($"{PluginStepRegistrationEntityNames.SdkMessageFilter}.{SdkMessageFilterFieldNames.PrimaryObjectTypeCode}");

                return primaryObjectTypeCode == null || entityTypeCode.HasValue && (int)primaryObjectTypeCode.Value == entityTypeCode.Value;
            });

            var pluginStepDefinitions = pluginSteps
                .Select(ps => new PluginStepDefinition()
                {
                    Id = ps.Id,
                    FilteringAttributes = !string.IsNullOrEmpty(ps.GetAttributeValue<string>(SdkMessageProcessingStepFieldNames.FilteringAttributes))
                                                                ? ps.GetAttributeValue<string>(SdkMessageProcessingStepFieldNames.FilteringAttributes)
                                                                        .ToLowerInvariant()
                                                                        .Split(',')
                                                                        .ToList()
                                                                : new List<string>(),
                    Stage = stage,
                    Mode = mode,
                    Rank = (int)ps[SdkMessageProcessingStepFieldNames.Rank],
                    MessageName = requestName,
                    EntityTypeCode = (int?)ps.GetAttributeValue<AliasedValue>($"{PluginStepRegistrationEntityNames.SdkMessageFilter}.{SdkMessageFilterFieldNames.PrimaryObjectTypeCode}")?.Value,
                    AssemblyName = (string)ps.GetAttributeValue<AliasedValue>($"{PluginStepRegistrationEntityNames.PluginType}.{PluginTypeFieldNames.AssemblyName}").Value,
                    PluginType = (string)ps.GetAttributeValue<AliasedValue>($"{PluginStepRegistrationEntityNames.PluginType}.{PluginTypeFieldNames.TypeName}")?.Value
                }).AsEnumerable();

            //Filter attributes
            return pluginStepDefinitions.Where(p => !p.FilteringAttributes.Any() || p.FilteringAttributes.Any(attr => entity.Attributes.ContainsKey(attr)));
        }

        private static IEnumerable<PluginStepDefinition> GetStepsForStage(this IXrmFakedContext context,
                                                                            string requestName,
                                                                            ProcessingStepStage stage,
                                                                            ProcessingStepMode mode,
                                                                            Entity entity)
        {
            int? entityTypeCode = null;
            string entityLogicalName = null;

            if (entity != null)
            {
                entityTypeCode = (int?)entity.GetType().GetField("EntityTypeCode")?.GetValue(entity);
                entityLogicalName = entity.LogicalName;
            }

            var pluginInstancesRepository = context.GetProperty<IPluginInstancesRepository>();
            
            var pluginSteps = (from step in context.CreateQuery(PluginStepRegistrationEntityNames.SdkMessageProcessingStep)
                               join message in context.CreateQuery(PluginStepRegistrationEntityNames.SdkMessage) on (step[SdkMessageProcessingStepFieldNames.SdkMessageId] as EntityReference).Id equals message.Id
                               join pluginAssembly in context.CreateQuery(PluginStepRegistrationEntityNames.PluginType) on (step[SdkMessageProcessingStepFieldNames.EventHandler] as EntityReference).Id equals pluginAssembly.Id
                               join mFilter in context.CreateQuery(PluginStepRegistrationEntityNames.SdkMessageFilter) on (step[SdkMessageProcessingStepFieldNames.SdkMessageFilterId] != null ? (step[SdkMessageProcessingStepFieldNames.SdkMessageFilterId] as EntityReference).Id : Guid.Empty) equals mFilter.Id into mesFilter
                               join stepSecConfig in context.CreateQuery(PluginStepRegistrationEntityNames.SdkMessageProcessingStepSecureConfig) on (step[SdkMessageProcessingStepFieldNames.SdkMessageProcessingStepSecureConfigId] != null ? (step[SdkMessageProcessingStepFieldNames.SdkMessageProcessingStepSecureConfigId] as EntityReference).Id : Guid.Empty) equals stepSecConfig.Id into secureConfig

                               from messageFilter in mesFilter.DefaultIfEmpty()
                               from secureConfiguration in secureConfig.DefaultIfEmpty()
                               
                               where (step[SdkMessageProcessingStepFieldNames.Stage] as OptionSetValue).Value == (int)stage
                               where (step[SdkMessageProcessingStepFieldNames.Mode] as OptionSetValue).Value == (int)mode
                               where (message[SdkMessageFieldNames.Name] as string) == requestName
                               select new PluginStepDefinition
                               {
                                   Id = step.Id,
                                   Rank = (int)step[SdkMessageProcessingStepFieldNames.Rank],
                                   Stage = stage,
                                   Mode = mode,
                                   MessageName = requestName,
                                   FilteringAttributes = step.GetPluginStepFilteringAttributes(),
                                   EntityTypeCode = messageFilter.GetMessageFilterPrimaryObjectCode(),
                                   EntityLogicalName = messageFilter.GetMessageFilterEntityLogicalName(),
                                   AssemblyName = pluginAssembly.GetAttributeValue<string>(PluginTypeFieldNames.AssemblyName),
                                   PluginType = pluginAssembly.GetAttributeValue<string>(PluginTypeFieldNames.TypeName),
                                   Configurations = secureConfiguration != null ? new PluginStepConfigurations()
                                   {
                                       SecureConfigId = secureConfiguration.Id,
                                       SecureConfig = secureConfiguration.GetAttributeValue<string>(SdkMessageProcessingStepSecureConfigFieldNames.SecureConfig),
                                       UnsecureConfig = step.GetAttributeValue<string>(SdkMessageProcessingStepFieldNames.Configuration)
                                   } : null,
                                   PluginInstance = pluginInstancesRepository.GetPluginInstance(step.Id)
                               }).OrderBy(ps => ps.Rank)
                                 .ToList();

            return pluginSteps
                        .Where(ps => ps.EntityLogicalName != null && ps.EntityLogicalName == entityLogicalName || //Matches logical name
                                        ps.EntityTypeCode != null && ps.EntityTypeCode.HasValue && ps.EntityTypeCode.Value == entityTypeCode || //Or matches entity type code
                                        ps.EntityTypeCode == null && ps.EntityLogicalName == null) //Or matches plugins steps with none (Custom Apis)
                        .Where(ps => !ps.FilteringAttributes.Any() || ps.FilteringAttributes.Any(attr => entity.Attributes.ContainsKey(attr))).AsEnumerable();
        }

        internal static IEnumerable<Entity> GetPluginImageDefinitions(this IXrmFakedContext context, Guid stepId, ProcessingStepImageType imageType)
        {
            return context.GetPluginImageDefinitionsWithQuery(stepId, imageType);
        }


        internal static IEnumerable<Entity> GetPluginImageDefinitionsWithRetrieveMultiple(this IXrmFakedContext context, Guid stepId, ProcessingStepImageType imageType)
        {
            var query = new QueryExpression(PluginStepRegistrationEntityNames.SdkMessageProcessingStepImage)
            {
                ColumnSet = new ColumnSet(SdkMessageProcessingStepImageFieldNames.Name, SdkMessageProcessingStepImageFieldNames.ImageType, SdkMessageProcessingStepImageFieldNames.Attributes),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(SdkMessageProcessingStepImageFieldNames.SdkMessageProcessingStepId, ConditionOperator.Equal, stepId)
                    }
                }
            };

            FilterExpression filter = new FilterExpression(LogicalOperator.Or)
            {
                Conditions = { new ConditionExpression(SdkMessageProcessingStepImageFieldNames.ImageType, ConditionOperator.Equal, (int)ProcessingStepImageType.Both) }
            };

            if (imageType == ProcessingStepImageType.PreImage || imageType == ProcessingStepImageType.PostImage)
            {
                filter.AddCondition(new ConditionExpression(SdkMessageProcessingStepImageFieldNames.ImageType, ConditionOperator.Equal, (int)imageType));
            }

            query.Criteria.AddFilter(filter);

            return context.GetOrganizationService().RetrieveMultiple(query).Entities.AsEnumerable();
        }

        internal static IEnumerable<Entity> GetPluginImageDefinitionsWithQuery(this IXrmFakedContext context, Guid stepId, ProcessingStepImageType imageType)
        {
            return (from pluginStepImage in context.CreateQuery(PluginStepRegistrationEntityNames.SdkMessageProcessingStepImage)
                    where ((EntityReference)pluginStepImage[SdkMessageProcessingStepImageFieldNames.SdkMessageProcessingStepId]).Id == stepId
                    where ((OptionSetValue)pluginStepImage[SdkMessageProcessingStepImageFieldNames.ImageType]).Value == (int)ProcessingStepImageType.Both
                            || ((imageType == ProcessingStepImageType.PreImage || imageType == ProcessingStepImageType.PostImage) &&
                                    ((OptionSetValue)pluginStepImage[SdkMessageProcessingStepImageFieldNames.ImageType]).Value == (int)imageType)
                    select pluginStepImage).AsEnumerable();
        }

        private static EntityImageCollection GetEntityImageCollection(IEnumerable<Entity> imageDefinitions, Entity values)
        {
            EntityImageCollection collection = new EntityImageCollection();

            if (values != null && imageDefinitions != null)
            {
                foreach (Entity imageDefinition in imageDefinitions)
                {
                    string name = imageDefinition.GetAttributeValue<string>(SdkMessageProcessingStepImageFieldNames.Name);
                    if (string.IsNullOrEmpty(name))
                    {
                        name = string.Empty;
                    }

                    string attributes = imageDefinition.GetAttributeValue<string>(SdkMessageProcessingStepImageFieldNames.Attributes);

                    Entity image = values.Clone(values.GetType());
                    if (!string.IsNullOrEmpty(attributes))
                    {
                        string[] specifiedAttributes = attributes.Split(',');

                        foreach (KeyValuePair<string, object> attr in values.Attributes.Where(x => !specifiedAttributes.Contains(x.Key)))
                        {
                            image.Attributes.Remove(attr.Key);
                        }
                    }

                    collection.Add(name, image);
                }
            }

            return collection;
        }

        internal static object GetTargetForRequest(OrganizationRequest request)
        {
            if (request is CreateRequest)
            {
                return ((CreateRequest)request).Target;
            }
            else if (request is UpdateRequest)
            {
                return ((UpdateRequest)request).Target;
            }
#if FAKE_XRM_EASY_2015 || FAKE_XRM_EASY_2016 || FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
            else if(request is UpsertRequest)
            {
                return ((UpsertRequest) request).Target;
            }
#endif
            else if (request is RetrieveRequest)
            {
                return ((RetrieveRequest)request).Target;
            }
            else if (request is DeleteRequest)
            {
                return ((DeleteRequest)request).Target;
            }
            return null;
        }


    }
}
