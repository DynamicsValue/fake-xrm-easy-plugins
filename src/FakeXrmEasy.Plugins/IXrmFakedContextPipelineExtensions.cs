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
            if(entityType.IsSubclassOf(typeof(Entity)))
            {
                var entityTypeCodeField = entityType.GetField("EntityTypeCode");
                if(entityTypeCodeField == null)
                {
                    throw new EntityTypeCodeNotFoundException(entity.LogicalName);
                }
                var entityTypeCode = (int)entityTypeCodeField.GetValue(entity);

                var pluginStepDefinition = new PluginStepDefinition()
                {
                    MessageName = message,
                    Stage = stage,
                    Mode = mode,
                    Rank = rank,
                    FilteringAttributes = filteringAttributes,
                    EntityTypeCode = entityTypeCode,
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

            if(pluginStepDefinition.Id != Guid.Empty &&
                context.ContainsEntity(PluginStepRegistrationEntityNames.SdkMessageProcessingStep, pluginStepDefinition.Id))
            {
                throw new PluginStepDefinitionAlreadyRegisteredException(pluginStepDefinition.Id);
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

        internal static Guid RegisterPluginStepInternal(this IXrmFakedContext context,
                                                                Type pluginType,
                                                                IPluginStepDefinition pluginStepDefinition)

        {
            ValidatePluginStep(context, pluginStepDefinition);

            // Message and MessageFilter
            var sdkMessage = AddOrUseSdkMessage(context, pluginStepDefinition);
            var sdkMessageFilter = AddSdkMessageFilter(context, pluginStepDefinition);

            // Store Plugin Type as a record
            var assemblyName = pluginType.Assembly.GetName();
            var pluginTypeRecord = context.AddPluginType(pluginType, assemblyName);

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
                [SdkMessageProcessingStepFieldNames.Rank] = pluginStepDefinition.Rank
            };
            context.AddEntityWithDefaults(sdkMessageProcessingStep);

            AddSdkMessageProcessingStepImages(context, pluginStepDefinition, sdkMessageProcessingStep);

            return sdkMessageProcessingStepId;
        }

        internal static void ExecutePipelineStage(this IXrmFakedContext context, 
            PipelineStageExecutionParameters parameters,
            object target)
        {
            var plugins = context.GetPluginStepsForOrganizationRequest(parameters.RequestName, parameters.Stage, parameters.Mode, parameters.Request);
            if(plugins == null)
                return;

            if (target == null)
            {
                target = GetTargetForRequest(parameters.Request);
            }

            if (target is Entity)
            {
                parameters.Entity = target as Entity;
            }
            else if (target is EntityReference)
            {
                parameters.EntityReference = target as EntityReference;
            }

            context.ExecutePipelineStage(parameters);
        }

        internal static IEnumerable<PluginStepDefinition> GetPluginStepsForOrganizationRequestWithRetrieveMultiple(this IXrmFakedContext context, string requestName, ProcessingStepStage stage, ProcessingStepMode mode, OrganizationRequest request)
        {
            var target = GetTargetForRequest(request);
            if(target is Entity)
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

            return null;
        }

        private static void ExecutePipelineStage(this IXrmFakedContext context, PipelineStageExecutionParameters parameters)
        {
            if(parameters.Entity != null)
            {
                var plugins = context.GetStepsForStage(parameters.RequestName, parameters.Stage, parameters.Mode, parameters.Entity);
                context.ExecutePipelinePlugins(plugins, parameters.Entity, parameters.PreEntitySnapshot, parameters.PostEntitySnapshot, parameters.Response);
            }
            else if(parameters.EntityReference != null)
            {
                var entityType = context.FindReflectedType(parameters.EntityReference.LogicalName);
                if (entityType == null)
                {
                    return;
                }

                var plugins = context.GetStepsForStage(parameters.RequestName, parameters.Stage, parameters.Mode, (Entity)Activator.CreateInstance(entityType));
                context.ExecutePipelinePlugins(plugins, parameters.EntityReference, parameters.PreEntitySnapshot, parameters.PostEntitySnapshot, parameters.Response);
            }           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="pluginSteps"></param>
        /// <param name="target"></param>
        /// <param name="previousValues"></param>
        /// <param name="resultingAttributes"></param>
        /// <param name="organizationResponse">The organization response that triggered this plugin execution</param>
        private static void ExecutePipelinePlugins(this IXrmFakedContext context, 
                                                    IEnumerable<PluginStepDefinition> pluginSteps, 
                                                    object target, 
                                                    Entity previousValues, 
                                                    Entity resultingAttributes,
                                                    OrganizationResponse organizationResponse)
        {
            var isAuditEnabled = context.GetProperty<PipelineOptions>().UsePluginStepAudit;

            foreach (var pluginStep in pluginSteps)
            {
                var pluginMethod = GetPluginMethod(pluginStep);

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
                pluginContext.Mode = (int) pluginStep.Mode;
                pluginContext.Stage = (int) pluginStep.Stage;
                pluginContext.MessageName = pluginStep.MessageName;
                pluginContext.InputParameters = new ParameterCollection
                {
                    { "Target", target }
                };
                pluginContext.OutputParameters = organizationResponse != null ? organizationResponse.Results : new ParameterCollection();
                pluginContext.PreEntityImages = GetEntityImageCollection(preImageDefinitions, previousValues);
                pluginContext.PostEntityImages = GetEntityImageCollection(postImageDefinitions, resultingAttributes);

                pluginMethod.Invoke(null, new object[] { context, pluginContext });

                if(isAuditEnabled)
                {
                    context.AddPluginStepAuditDetails(pluginMethod, pluginContext, pluginStep, target);
                }
            }
        }

        private static void AddPluginStepAuditDetails(this IXrmFakedContext context, 
                                MethodInfo pluginMethod, 
                                XrmFakedPluginExecutionContext pluginContext, 
                                PluginStepDefinition pluginStep, 
                                object target)
        {
            var pluginType = pluginMethod.GetGenericArguments()[0];
            var pluginStepAuditDetails = new PluginStepAuditDetails()
            {
                PluginAssemblyType = pluginType,
                PluginStepId = pluginStep.Id,
                MessageName = pluginContext.MessageName,
                Stage = (ProcessingStepStage)pluginContext.Stage,
                InputParameters = pluginContext.InputParameters,
                OutputParameters = pluginContext.OutputParameters
            };
            
            if (target is Entity) 
                pluginStepAuditDetails.TargetEntity = (Entity) target;
            
            if (target is EntityReference)
                pluginStepAuditDetails.TargetEntityReference = (EntityReference)target;


            var pluginStepAudit = context.GetProperty<IPluginStepAudit>() as PluginStepAudit;
            pluginStepAudit.Add(pluginStepAuditDetails);
        }

        private static MethodInfo GetPluginMethod(PluginStepDefinition pluginStepDefinition)
        {
            var assembly = AppDomain.CurrentDomain.Load(pluginStepDefinition.AssemblyName);
            var pluginType = assembly.GetType(pluginStepDefinition.PluginType);

            var methodInfo = typeof(IXrmBaseContextPluginExtensions).GetMethod("ExecutePluginWith", new[] { typeof(IXrmFakedContext), typeof(XrmFakedPluginExecutionContext) });
            var pluginMethod = methodInfo.MakeGenericMethod(pluginType);

            return pluginMethod;
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
            var entityTypeCode = (int?)entity.GetType().GetField("EntityTypeCode")?.GetValue(entity);
            var entityLogicalName = entity.LogicalName;

            var pluginSteps = (from step in context.CreateQuery(PluginStepRegistrationEntityNames.SdkMessageProcessingStep)
                               join message in context.CreateQuery(PluginStepRegistrationEntityNames.SdkMessage) on (step[SdkMessageProcessingStepFieldNames.SdkMessageId] as EntityReference).Id equals message.Id
                               join pluginAssembly in context.CreateQuery(PluginStepRegistrationEntityNames.PluginType) on (step[SdkMessageProcessingStepFieldNames.EventHandler] as EntityReference).Id equals pluginAssembly.Id
                               join mFilter in context.CreateQuery(PluginStepRegistrationEntityNames.SdkMessageFilter) on (step[SdkMessageProcessingStepFieldNames.SdkMessageFilterId] != null ? (step[SdkMessageProcessingStepFieldNames.SdkMessageFilterId] as EntityReference).Id : Guid.Empty) equals mFilter.Id into mesFilter
                               from messageFilter in mesFilter.DefaultIfEmpty()

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
                                   PluginType = pluginAssembly.GetAttributeValue<string>(PluginTypeFieldNames.TypeName)
                               }).OrderBy(ps => ps.Rank)
                                 .ToList();

            return pluginSteps
                        .Where(ps => ps.EntityLogicalName != null && ps.EntityLogicalName == entityLogicalName || //Matches logical name
                                        ps.EntityTypeCode != null && ps.EntityTypeCode.HasValue && ps.EntityTypeCode.Value == entityTypeCode || //Or matches entity type code
                                        ps.EntityTypeCode == null && ps.EntityLogicalName == null) //Or matches plugins steps with none
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
                    where ((OptionSetValue) pluginStepImage[SdkMessageProcessingStepImageFieldNames.ImageType]).Value == (int)ProcessingStepImageType.Both 
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
            if(request is CreateRequest)
            {
                return ((CreateRequest) request).Target;
            }
            else if(request is UpdateRequest)
            {
                return ((UpdateRequest) request).Target;
            }
#if FAKE_XRM_EASY_2015 || FAKE_XRM_EASY_2016 || FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
            else if(request is UpsertRequest)
            {
                return ((UpsertRequest) request).Target;
            }
#endif
            else if(request is RetrieveRequest)
            {
                return ((RetrieveRequest) request).Target;
            }
            else if(request is DeleteRequest)
            {
                return ((DeleteRequest) request).Target;
            }
            return null;
        }

        
    }
}
