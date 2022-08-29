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

namespace FakeXrmEasy.Pipeline
{
    /// <summary>
    /// Extension methods to register plugin steps
    /// </summary>
    public static class IXrmFakedContextPipelineExtensions
    {
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
            return context.RegisterPluginStepInternal<TPlugin>(message, stage, mode, rank, filteringAttributes, null, entityLogicalName, registeredImages);
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
                return context.RegisterPluginStepInternal<TPlugin>(message, stage, mode, rank, filteringAttributes, entityTypeCode, entity.LogicalName, registeredImages);
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
            return RegisterPluginStepInternal<TPlugin>(context, message, stage, mode, rank: rank, 
                                                        filteringAttributes: filteringAttributes, 
                                                        primaryEntityTypeCode: primaryEntityTypeCode, 
                                                        null,
                                                        registeredImages: registeredImages);
        }

        internal static Guid RegisterPluginStepInternal<TPlugin>(this IXrmFakedContext context, 
                                                                string message, 
                                                                ProcessingStepStage stage = ProcessingStepStage.Postoperation, 
                                                                ProcessingStepMode mode = ProcessingStepMode.Synchronous, 
                                                                int rank = 1, 
                                                                string[] filteringAttributes = null, 
                                                                int? primaryEntityTypeCode = null, 
                                                                string entityLogicalName = null,
                                                                IEnumerable<PluginImageDefinition> registeredImages = null)
            where TPlugin : IPlugin

        {
            if(context.HasProperty<PipelineOptions>())
            {
                var pipelineOptions = context.GetProperty<PipelineOptions>();
                if (pipelineOptions.UsePluginStepRegistrationValidation)
                {
                    var validator = context.GetProperty<IPluginStepValidator>();
                    bool isValid = true;

                    if(primaryEntityTypeCode == null && string.IsNullOrWhiteSpace(entityLogicalName))
                    {
                        isValid = validator.IsValid(message, "*", stage, mode);
                    }
                    else if(!string.IsNullOrWhiteSpace(entityLogicalName))
                    {
                        isValid = validator.IsValid(message, entityLogicalName, stage, mode);
                    }
                    if (!isValid)
                    {
                        throw new InvalidPluginStepRegistrationException();
                    }
                }
            }

            // Message
            var sdkMessage = context
                                .CreateQuery(PluginStepRegistrationEntityNames.SdkMessage)
                                .FirstOrDefault(sm => string.Equals(sm.GetAttributeValue<string>(SdkMessageFieldNames.Name), message));

            if (sdkMessage == null)
            {
                sdkMessage = new Entity(PluginStepRegistrationEntityNames.SdkMessage)
                {
                    Id = Guid.NewGuid(),
                    [SdkMessageFieldNames.Name] = message
                };
                context.AddEntityWithDefaults(sdkMessage);
            }

            // Plugin Type
            var type = typeof(TPlugin);
            var assemblyName = type.Assembly.GetName();

            var pluginType = context
                                .CreateQuery(PluginStepRegistrationEntityNames.PluginType)
                                .FirstOrDefault(pt => string.Equals(pt.GetAttributeValue<string>(PluginTypeFieldNames.TypeName), type.FullName) 
                                                    && string.Equals(pt.GetAttributeValue<string>(PluginTypeFieldNames.AssemblyName), assemblyName.Name));
            if (pluginType == null)
            {
                pluginType = new Entity(PluginStepRegistrationEntityNames.PluginType)
                {
                    Id = Guid.NewGuid(),
                    [PluginTypeFieldNames.Name] = type.FullName,
                    [PluginTypeFieldNames.TypeName] = type.FullName,
                    [PluginTypeFieldNames.AssemblyName] = assemblyName.Name,
                    [PluginTypeFieldNames.Major] = assemblyName.Version.Major,
                    [PluginTypeFieldNames.Minor] = assemblyName.Version.Minor,
                    [PluginTypeFieldNames.Version] = assemblyName.Version.ToString(),
                };
                context.AddEntityWithDefaults(pluginType);
            }

            // Filter
            Entity sdkFilter = null;
            if (primaryEntityTypeCode.HasValue)
            {
                sdkFilter = new Entity(PluginStepRegistrationEntityNames.SdkMessageFilter)
                {
                    Id = Guid.NewGuid(),
                    [SdkMessageFilterFieldNames.EntityLogicalName] = entityLogicalName,
                    [SdkMessageFilterFieldNames.PrimaryObjectTypeCode] = primaryEntityTypeCode
                };
                context.AddEntityWithDefaults(sdkFilter);
            }

            // Message Step
            var sdkMessageProcessingStepId = Guid.NewGuid();

            var sdkMessageProcessingStep = new Entity(PluginStepRegistrationEntityNames.SdkMessageProcessingStep)
            {
                Id = sdkMessageProcessingStepId,
                [SdkMessageProcessingStepFieldNames.EventHandler] = pluginType.ToEntityReference(),
                [SdkMessageProcessingStepFieldNames.SdkMessageId] = sdkMessage.ToEntityReference(),
                [SdkMessageProcessingStepFieldNames.SdkMessageFilterId] = sdkFilter?.ToEntityReference(),
                [SdkMessageProcessingStepFieldNames.FilteringAttributes] = filteringAttributes != null ? string.Join(",", filteringAttributes) : null,
                [SdkMessageProcessingStepFieldNames.Mode] = new OptionSetValue((int)mode),
                [SdkMessageProcessingStepFieldNames.Stage] = new OptionSetValue((int)stage),
                [SdkMessageProcessingStepFieldNames.Rank] = rank
            };
            context.AddEntityWithDefaults(sdkMessageProcessingStep);

            //Images setup
            if (registeredImages != null)
            {
                foreach (var pluginImage in registeredImages)
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

            return sdkMessageProcessingStepId;
        }

        internal static void ExecutePipelineStage(this IXrmFakedContext context, string requestName, ProcessingStepStage stage, ProcessingStepMode mode, 
            OrganizationRequest request, object target = null, Entity preEntity = null, Entity postEntity = null)
        {
            var plugins = context.GetPluginStepsForOrganizationRequest(requestName, stage, mode, request);
            if(plugins == null)
                return;

            if (target == null)
            {
                target = GetTargetForRequest(request);
            }

            if (target is Entity)
            {
                context.ExecutePipelineStage(requestName, stage, mode, target as Entity, preEntity, postEntity);
            }
            else if (target is EntityReference)
            {
                context.ExecutePipelineStage(requestName, stage, mode, target as EntityReference, preEntity, postEntity);
            }
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

        private static void ExecutePipelineStage(this IXrmFakedContext context, string method, ProcessingStepStage stage, ProcessingStepMode mode, 
                                                Entity entity, Entity previousValues = null, Entity resultingAttributes = null)
        {
            var plugins = context.GetStepsForStage(method, stage, mode, entity);
            context.ExecutePipelinePlugins(plugins, entity, previousValues, resultingAttributes);
        }

        
        private static void ExecutePipelineStage(this IXrmFakedContext context, string method, ProcessingStepStage stage, ProcessingStepMode mode, 
                                                EntityReference entityReference, Entity previousValues = null, Entity resultingAttributes = null)
        {
            var entityType = context.FindReflectedType(entityReference.LogicalName);
            if (entityType == null)
            {
                return;
            }

            var plugins = context.GetStepsForStage(method, stage, mode, (Entity)Activator.CreateInstance(entityType));

            context.ExecutePipelinePlugins(plugins, entityReference, previousValues, resultingAttributes);
        }

        private static void ExecutePipelinePlugins(this IXrmFakedContext context, IEnumerable<PluginStepDefinition> pluginSteps, object target, Entity previousValues, Entity resultingAttributes)
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
                pluginContext.OutputParameters = new ParameterCollection();
                pluginContext.PreEntityImages = GetEntityImageCollection(preImageDefinitions, previousValues);
                pluginContext.PostEntityImages = GetEntityImageCollection(postImageDefinitions, resultingAttributes);

                pluginMethod.Invoke(null, new object[] { context, pluginContext });

                if(isAuditEnabled)
                {
                    context.AddPluginStepAuditDetails(pluginMethod, pluginContext, pluginStep, target);
                }
            }
        }

        private static void AddPluginStepAuditDetails(this IXrmFakedContext context, MethodInfo pluginMethod, XrmFakedPluginExecutionContext pluginContext, PluginStepDefinition pluginStep, object target)
        {
            var pluginType = pluginMethod.GetGenericArguments()[0];
            var pluginStepAuditDetails = new PluginStepAuditDetails()
            {
                PluginAssemblyType = pluginType,
                PluginStepId = pluginStep.Id,
                MessageName = pluginContext.MessageName,
                Stage = (ProcessingStepStage)pluginContext.Stage
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

            var methodInfo = typeof(IXrmFakedContextPluginExtensions).GetMethod("ExecutePluginWith", new[] { typeof(IXrmFakedContext), typeof(XrmFakedPluginExecutionContext) });
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
            return pluginStepDefinitions.Where(p => p.FilteringAttributes.Count == 0 || p.FilteringAttributes.Any(attr => entity.Attributes.ContainsKey(attr)));
        }

        private static IEnumerable<PluginStepDefinition> GetStepsForStage(this IXrmFakedContext context, 
                                                                            string requestName, 
                                                                            ProcessingStepStage stage, 
                                                                            ProcessingStepMode mode, 
                                                                            Entity entity)
        {
            var entityTypeCode = (int?)entity.GetType().GetField("EntityTypeCode")?.GetValue(entity);

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
                                   FilteringAttributes = !string.IsNullOrEmpty(step.GetAttributeValue<string>(SdkMessageProcessingStepFieldNames.FilteringAttributes)) 
                                                            ? step.GetAttributeValue<string>(SdkMessageProcessingStepFieldNames.FilteringAttributes)
                                                                .ToLowerInvariant()
                                                                .Split(',')
                                                                .ToList() 
                                                            : new List<string>(),
                                   EntityTypeCode = messageFilter != null 
                                                ? (int) messageFilter[SdkMessageFilterFieldNames.PrimaryObjectTypeCode] 
                                                : new int?(),
                                   AssemblyName = pluginAssembly.GetAttributeValue<string>(PluginTypeFieldNames.AssemblyName),
                                   PluginType = pluginAssembly.GetAttributeValue<string>(PluginTypeFieldNames.TypeName)
                               }).OrderBy(ps => ps.Rank)
                                 .ToList();

            return pluginSteps
                        .Where(ps => ps.EntityTypeCode == null || ps.EntityTypeCode.HasValue && ps.EntityTypeCode.Value == entityTypeCode)
                        .Where(ps => ps.FilteringAttributes.Count == 0 || ps.FilteringAttributes.Any(attr => entity.Attributes.ContainsKey(attr))).AsEnumerable();
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

                    Entity preImage = values.Clone(values.GetType());
                    if (!string.IsNullOrEmpty(attributes))
                    {
                        string[] specifiedAttributes = attributes.Split(',');

                        foreach (KeyValuePair<string, object> attr in values.Attributes.Where(x => !specifiedAttributes.Contains(x.Key)))
                        {
                            preImage.Attributes.Remove(attr.Key);
                        }
                    }

                    collection.Add(name, preImage);
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
