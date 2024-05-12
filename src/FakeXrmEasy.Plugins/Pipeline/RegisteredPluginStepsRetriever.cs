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
    internal static class RegisteredPluginStepsRetriever
    {
        internal static IEnumerable<PluginStepDefinition> GetPluginStepsForOrganizationRequest(IXrmFakedContext context, PipelineStageExecutionParameters parameters)
        {
            var target = GetTargetForRequest(parameters.Request);
            if (target is Entity)
            {
                return GetStepsForStage(context, parameters, target as Entity);
            }

            if (target is EntityReference)
            {
                var entityReference = target as EntityReference;
                var entityType = context.FindReflectedType(entityReference.LogicalName);
                Entity entity = null;
                if (entityType == null)
                {
                    entity = new Entity(entityReference.LogicalName) { Id = entityReference.Id };
                }
                else
                {
                    entity = (Entity)Activator.CreateInstance(entityType);
                    entity.Id = entityReference.Id;
                }

                return GetStepsForStage(context, parameters, entity);
            }

            //Possibly a custom api execution
            return GetStepsForStage(context, parameters, null);
        }

        /// <summary>
        /// Returns a distinct attribute array that was sent in a given organization request
        /// </summary>
        /// <returns></returns>
        internal static string[] GetOrganizationRequestFilteringAttributes(OrganizationRequest request)
        {
            var target = GetTargetForRequest(request);
            var entity = target as Entity;
            if (entity != null)
            {
                return entity.Attributes.Keys.ToArray();
            }

            var targets = GetTargetsForRequest(request);
            var entityCollection = targets as EntityCollection;
            if (entityCollection != null)
            {
                List<string> attributes = new List<string>();
                foreach (var e in entityCollection.Entities)
                {
                    attributes.AddRange(e.Attributes.Keys);
                }
                return attributes.Distinct().ToArray();
            }
            
            return new string[] { };
        }
        
        private static IEnumerable<PluginStepDefinition> GetStepsForStage(IXrmFakedContext context,
                                                                            PipelineStageExecutionParameters parameters,
                                                                            Entity entity)
        {
            int? entityTypeCode = null;
            string entityLogicalName = null;

            var requestDistinctAttributes = GetOrganizationRequestFilteringAttributes(parameters.Request);
            
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
                               
                               where (step[SdkMessageProcessingStepFieldNames.Stage] as OptionSetValue).Value == (int)parameters.Stage
                               where (step[SdkMessageProcessingStepFieldNames.Mode] as OptionSetValue).Value == (int)parameters.Mode
                               where (message[SdkMessageFieldNames.Name] as string) == parameters.Request.RequestName
                               select new PluginStepDefinition
                               {
                                   Id = step.Id,
                                   Rank = (int)step[SdkMessageProcessingStepFieldNames.Rank],
                                   Stage = parameters.Stage,
                                   Mode = parameters.Mode,
                                   MessageName = parameters.Request.RequestName,
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
                        .Where(ps => !ps.FilteringAttributes.Any() || ps.FilteringAttributes.Any(attr => requestDistinctAttributes.Contains(attr))).AsEnumerable();
        }

        internal static IEnumerable<Entity> GetPluginImageDefinitions(IXrmFakedContext context, Guid stepId, ProcessingStepImageType imageType)
        {
            return GetPluginImageDefinitionsWithQuery(context, stepId, imageType);
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

        internal static object GetTargetForRequest(OrganizationRequest request)
        {
            if (request.Parameters.ContainsKey("Target"))
            {
                return request.Parameters["Target"];
            }

            return null;
        }
        
        internal static object GetTargetsForRequest(OrganizationRequest request)
        {
            if (request.Parameters.ContainsKey("Targets"))
            {
                return request.Parameters["Targets"];
            }

            return null;
        }
    }
}