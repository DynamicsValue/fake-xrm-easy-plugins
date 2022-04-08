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

namespace FakeXrmEasy.Pipeline
{
    /// <summary>
    /// Extension methods to register plugin steps
    /// </summary>
    public static class IXrmFakedContextPipelineExtensions
    {
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
        public static Guid RegisterPluginStep<TPlugin, TEntity>(this IXrmFakedContext context, string message, ProcessingStepStage stage = ProcessingStepStage.Postoperation, ProcessingStepMode mode = ProcessingStepMode.Synchronous, int rank = 1, string[] filteringAttributes = null, IEnumerable<PluginImageDefinition> registeredImages = null)
            where TPlugin : IPlugin
            where TEntity : Entity, new()
        {
            var entity = new TEntity();
            var entityTypeCode = (int)entity.GetType().GetField("EntityTypeCode").GetValue(entity);

            return context.RegisterPluginStep<TPlugin>(message, stage, mode, rank, filteringAttributes, entityTypeCode, registeredImages);
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
        public static Guid RegisterPluginStep<TPlugin>(this IXrmFakedContext context, string message, ProcessingStepStage stage = ProcessingStepStage.Postoperation, ProcessingStepMode mode = ProcessingStepMode.Synchronous, int rank = 1, string[] filteringAttributes = null, int? primaryEntityTypeCode = null, IEnumerable<PluginImageDefinition> registeredImages = null)
            where TPlugin : IPlugin
        {
            // Message
            var sdkMessage = context.CreateQuery("sdkmessage").FirstOrDefault(sm => string.Equals(sm.GetAttributeValue<string>("name"), message));
            if (sdkMessage == null)
            {
                sdkMessage = new Entity("sdkmessage")
                {
                    Id = Guid.NewGuid(),
                    ["name"] = message
                };
                context.AddEntityWithDefaults(sdkMessage);
            }

            // Plugin Type
            var type = typeof(TPlugin);
            var assemblyName = type.Assembly.GetName();

            var pluginType = context.CreateQuery("plugintype").FirstOrDefault(pt => string.Equals(pt.GetAttributeValue<string>("typename"), type.FullName) && string.Equals(pt.GetAttributeValue<string>("asemblyname"), assemblyName.Name));
            if (pluginType == null)
            {
                pluginType = new Entity("plugintype")
                {
                    Id = Guid.NewGuid(),
                    ["name"] = type.FullName,
                    ["typename"] = type.FullName,
                    ["assemblyname"] = assemblyName.Name,
                    ["major"] = assemblyName.Version.Major,
                    ["minor"] = assemblyName.Version.Minor,
                    ["version"] = assemblyName.Version.ToString(),
                };
                context.AddEntityWithDefaults(pluginType);
            }

            // Filter
            Entity sdkFilter = null;
            if (primaryEntityTypeCode.HasValue)
            {
                sdkFilter = new Entity("sdkmessagefilter")
                {
                    Id = Guid.NewGuid(),
                    ["primaryobjecttypecode"] = primaryEntityTypeCode
                };
                context.AddEntityWithDefaults(sdkFilter);
            }

            // Message Step
            var sdkMessageProcessingStepId = Guid.NewGuid();

            var sdkMessageProcessingStep = new Entity("sdkmessageprocessingstep")
            {
                Id = sdkMessageProcessingStepId,
                ["eventhandler"] = pluginType.ToEntityReference(),
                ["sdkmessageid"] = sdkMessage.ToEntityReference(),
                ["sdkmessagefilterid"] = sdkFilter?.ToEntityReference(),
                ["filteringattributes"] = filteringAttributes != null ? string.Join(",", filteringAttributes) : null,
                ["mode"] = new OptionSetValue((int)mode),
                ["stage"] = new OptionSetValue((int)stage),
                ["rank"] = rank
            };
            context.AddEntityWithDefaults(sdkMessageProcessingStep);

            //Images setup
            if(registeredImages != null)
            {
                foreach (var pluginImage in registeredImages)
                {
                    var sdkMessageProcessingStepImage = new Entity("sdkmessageprocessingstepimage")
                    {
                        Id = Guid.NewGuid(),
                        ["name"] = pluginImage.Name,
                        ["sdkmessageprocessingstepid"] = sdkMessageProcessingStep.ToEntityReference(),
                        ["imagetype"] = new OptionSetValue((int)pluginImage.ImageType),
                        ["attributes"] = pluginImage.Attributes != null ? string.Join(",", pluginImage.Attributes) : null,
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
            var query = new QueryExpression("sdkmessageprocessingstep")
            {
                ColumnSet = new ColumnSet("configuration", "filteringattributes", "stage", "mode", "rank"),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("stage", ConditionOperator.Equal, (int)stage),
                        new ConditionExpression("mode", ConditionOperator.Equal, (int)mode)
                    }
                },
                Orders =
                {
                    new OrderExpression("rank", OrderType.Ascending)
                },
                LinkEntities =
                {
                    new LinkEntity("sdkmessageprocessingstep", "sdkmessagefilter", "sdkmessagefilterid", "sdkmessagefilterid", JoinOperator.LeftOuter)
                    {
                        EntityAlias = "sdkmessagefilter",
                        Columns = new ColumnSet("primaryobjecttypecode")
                    },
                    new LinkEntity("sdkmessageprocessingstep", "sdkmessage", "sdkmessageid", "sdkmessageid", JoinOperator.Inner)
                    {
                        EntityAlias = "sdkmessage",
                        Columns = new ColumnSet("name"),
                        LinkCriteria =
                        {
                            Conditions =
                            {
                                new ConditionExpression("name", ConditionOperator.Equal, requestName)
                            }
                        }
                    },
                    new LinkEntity("sdkmessageprocessingstep", "plugintype", "eventhandler", "plugintypeid", JoinOperator.Inner)
                    {
                        EntityAlias = "plugintype",
                        Columns = new ColumnSet("assemblyname", "typename")
                    }
                }
            };

            var entityTypeCode = (int?)entity.GetType().GetField("EntityTypeCode")?.GetValue(entity);
            var service = context.GetOrganizationService();
            var pluginSteps = service.RetrieveMultiple(query).Entities.AsEnumerable();
            pluginSteps = pluginSteps.Where(p =>
            {
                var primaryObjectTypeCode = p.GetAttributeValue<AliasedValue>("sdkmessagefilter.primaryobjecttypecode");

                return primaryObjectTypeCode == null || entityTypeCode.HasValue && (int)primaryObjectTypeCode.Value == entityTypeCode.Value;
            });

            var pluginStepDefinitions = pluginSteps
                .Select(ps => new PluginStepDefinition()
                {
                    Id = ps.Id,
                    FilteringAttributes = !string.IsNullOrEmpty(ps.GetAttributeValue<string>("filteringattributes")) ? ps.GetAttributeValue<string>("filteringattributes").ToLowerInvariant().Split(',').ToList() : new List<string>(),
                    Stage = stage,
                    Mode = mode,
                    Rank = (int)ps["rank"],
                    MessageName = requestName,
                    EntityTypeCode = (int?)ps.GetAttributeValue<AliasedValue>("sdkmessagefilter.primaryobjecttypecode")?.Value,
                    AssemblyName = (string)ps.GetAttributeValue<AliasedValue>("plugintype.assemblyname").Value,
                    PluginType = (string)ps.GetAttributeValue<AliasedValue>("plugintype.typename")?.Value
                }).AsEnumerable();

            //Filter attributes
            return pluginStepDefinitions.Where(p => p.FilteringAttributes.Count == 0 || p.FilteringAttributes.Any(attr => entity.Attributes.ContainsKey(attr)));
        }

        private static IEnumerable<PluginStepDefinition> GetStepsForStage(this IXrmFakedContext context, string requestName, ProcessingStepStage stage, ProcessingStepMode mode, Entity entity)
        {
            var entityTypeCode = (int?)entity.GetType().GetField("EntityTypeCode")?.GetValue(entity);

            var pluginSteps = (from step in context.CreateQuery("sdkmessageprocessingstep")
                               join message in context.CreateQuery("sdkmessage") on (step["sdkmessageid"] as EntityReference).Id equals message.Id
                               join pluginAssembly in context.CreateQuery("plugintype") on (step["eventhandler"] as EntityReference).Id equals pluginAssembly.Id
                               join mFilter in context.CreateQuery("sdkmessagefilter") on (step["sdkmessagefilterid"] != null ? (step["sdkmessagefilterid"] as EntityReference).Id : Guid.Empty) equals mFilter.Id into mesFilter
                               from messageFilter in mesFilter.DefaultIfEmpty()

                               where (step["stage"] as OptionSetValue).Value == (int)stage
                               where (step["mode"] as OptionSetValue).Value == (int)mode
                               where (message["name"] as string) == requestName
                               select new PluginStepDefinition
                               {
                                   Id = step.Id,
                                   Rank = (int)step["rank"],
                                   Stage = stage,
                                   Mode = mode,
                                   MessageName = requestName,
                                   FilteringAttributes = !string.IsNullOrEmpty(step.GetAttributeValue<string>("filteringattributes")) ? step.GetAttributeValue<string>("filteringattributes").ToLowerInvariant().Split(',').ToList() : new List<string>(),
                                   EntityTypeCode = messageFilter != null ? (int) messageFilter["primaryobjecttypecode"] : new int?(),
                                   AssemblyName = pluginAssembly.GetAttributeValue<string>("assemblyname"),
                                   PluginType = pluginAssembly.GetAttributeValue<string>("typename")
                               }).OrderBy(ps => ps.Rank)
                                 .ToList();

            return pluginSteps
                        .Where(ps => ps.EntityTypeCode == null || ps.EntityTypeCode.HasValue && ps.EntityTypeCode.Value == entityTypeCode)
                        .Where(ps => ps.FilteringAttributes.Count == 0 || ps.FilteringAttributes.Any(attr => entity.Attributes.ContainsKey(attr))).AsEnumerable();
        }

        internal static IEnumerable<Entity> GetPluginImageDefinitions(this IXrmFakedContext context, Guid stepId, ProcessingStepImageType imageType)
        {
            var query = new QueryExpression("sdkmessageprocessingstepimage")
            {
                ColumnSet = new ColumnSet("name", "imagetype", "attributes"),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("sdkmessageprocessingstepid", ConditionOperator.Equal, stepId)
                    }
                }
            };

            FilterExpression filter = new FilterExpression(LogicalOperator.Or)
            {
                Conditions = { new ConditionExpression("imagetype", ConditionOperator.Equal, (int)ProcessingStepImageType.Both) }
            };

            if (imageType == ProcessingStepImageType.PreImage || imageType == ProcessingStepImageType.PostImage)
            {
                filter.AddCondition(new ConditionExpression("imagetype", ConditionOperator.Equal, (int)imageType));
            }

            query.Criteria.AddFilter(filter);

            return context.GetOrganizationService().RetrieveMultiple(query).Entities.AsEnumerable();
        }

        private static EntityImageCollection GetEntityImageCollection(IEnumerable<Entity> imageDefinitions, Entity values)
        {
            EntityImageCollection collection = new EntityImageCollection();

            if (values != null && imageDefinitions != null)
            {
                foreach (Entity imageDefinition in imageDefinitions)
                {
                    string name = imageDefinition.GetAttributeValue<string>("name");
                    if (string.IsNullOrEmpty(name))
                    {
                        name = string.Empty;
                    }

                    string attributes = imageDefinition.GetAttributeValue<string>("attributes");

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
