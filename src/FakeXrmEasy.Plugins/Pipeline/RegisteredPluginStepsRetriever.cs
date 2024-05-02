using System;
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Plugins.PluginInstances;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.PluginSteps.Extensions;
using FakeXrmEasy.Plugins.PluginSteps.PluginStepRegistrationFieldNames;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Pipeline
{
    /// <summary>
    /// Given an organization request, retrieves all the plugin steps registered for that message
    /// </summary>
    internal class RegisteredPluginStepsRetriever
    {
        internal static IEnumerable<PluginStepDefinition> GetPluginStepsForOrganizationRequest(IXrmFakedContext context, string requestName, ProcessingStepStage stage, ProcessingStepMode mode, OrganizationRequest request)
        {
            var target = GetTargetForRequest(request);
            if (target is Entity)
            {
                return GetStepsForStage(context, requestName, stage, mode, target as Entity);
            }
            else if (target is EntityReference)
            {
                var entityReference = target as EntityReference;
                var entityType = context.FindReflectedType(entityReference.LogicalName);
                if (entityType == null)
                {
                    return null;
                }

                return GetStepsForStage(context, requestName, stage, mode, (Entity)Activator.CreateInstance(entityType));
            }
            else
            {
                //Possibly a custom api execution
                return GetStepsForStage(context, requestName, stage, mode, null);
            }
        }
        
        internal static IEnumerable<PluginStepDefinition> GetStepsForStageWithRetrieveMultiple(IXrmFakedContext context, string requestName, ProcessingStepStage stage, ProcessingStepMode mode, Entity entity)
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

        private static IEnumerable<PluginStepDefinition> GetStepsForStage(IXrmFakedContext context,
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

        internal static IEnumerable<Entity> GetPluginImageDefinitions(IXrmFakedContext context, Guid stepId, ProcessingStepImageType imageType)
        {
            return GetPluginImageDefinitionsWithQuery(context, stepId, imageType);
        }


        internal static IEnumerable<Entity> GetPluginImageDefinitionsWithRetrieveMultiple(IXrmFakedContext context, Guid stepId, ProcessingStepImageType imageType)
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

        internal static IEnumerable<Entity> GetPluginImageDefinitionsWithQuery(IXrmFakedContext context, Guid stepId, ProcessingStepImageType imageType)
        {
            return (from pluginStepImage in context.CreateQuery(PluginStepRegistrationEntityNames.SdkMessageProcessingStepImage)
                    where ((EntityReference)pluginStepImage[SdkMessageProcessingStepImageFieldNames.SdkMessageProcessingStepId]).Id == stepId
                    where ((OptionSetValue)pluginStepImage[SdkMessageProcessingStepImageFieldNames.ImageType]).Value == (int)ProcessingStepImageType.Both
                            || ((imageType == ProcessingStepImageType.PreImage || imageType == ProcessingStepImageType.PostImage) &&
                                    ((OptionSetValue)pluginStepImage[SdkMessageProcessingStepImageFieldNames.ImageType]).Value == (int)imageType)
                    select pluginStepImage).AsEnumerable();
        }
        
        [Obsolete("This method is obsolete, should no longer be used as it has been replaced by a more efficient method 'GetPluginStepsForOrganizationRequest' that instead directly queries the FakeXrmEasy internal in-memory database")]
        internal static IEnumerable<PluginStepDefinition> GetPluginStepsForOrganizationRequestWithRetrieveMultiple(IXrmFakedContext context, string requestName, ProcessingStepStage stage, ProcessingStepMode mode, OrganizationRequest request)
        {
            var target = GetTargetForRequest(request);
            if (target is Entity)
            {
                return RegisteredPluginStepsRetriever.GetStepsForStageWithRetrieveMultiple(context, requestName, stage, mode, target as Entity);
            }
            else if (target is EntityReference)
            {
                var entityReference = target as EntityReference;
                var entityType = context.FindReflectedType(entityReference.LogicalName);
                if (entityType == null)
                {
                    return null;
                }

                return RegisteredPluginStepsRetriever.GetStepsForStageWithRetrieveMultiple(context, requestName, stage, mode, (Entity)Activator.CreateInstance(entityType));
            }

            return null;
        }
        
        internal static object GetTargetForRequest(OrganizationRequest request)
        {
            if (request.Parameters.ContainsKey("Target"))
            {
                return request.Parameters["Target"];
            }

            return null;
        }
    }
}