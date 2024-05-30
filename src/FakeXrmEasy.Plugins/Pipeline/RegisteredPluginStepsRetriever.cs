using System;
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Plugins.Extensions;
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
        /// <summary>
        /// Gets all the plugin steps needed to execute the request specified in the pipeline stage execution parameter.
        /// Also implements the merged pipeline simulation where a bulk operation request might also trigger "non-bulk" plugins if they exist and were registered
        /// and vice-versa (a non-bulk request triggering a bulk operation plugin)
        /// </summary>
        /// <param name="context">The In-Memory XrmFakedContext</param>
        /// <param name="parameters">The pipeline stage execution parameter with info about the current event pipeline stage that is being executed</param>
        /// <returns></returns>
        internal static IEnumerable<PluginStepDefinition> GetPluginStepsForOrganizationRequest(IXrmFakedContext context, PipelineStageExecutionParameters parameters)
        {
            var steps = GetStepsForStage(context, parameters);
            if (parameters.Request.IsBulkOperation())
            {
                var nonBulkPipelineParameters = parameters.ToNonBulkPipelineExecutionParameters();
                foreach (var nonBulkPipelineStageExecutionParameter in nonBulkPipelineParameters)
                {
                    var nonBulkSteps = GetStepsForStage(context, nonBulkPipelineStageExecutionParameter);
                    steps.AddRange(nonBulkSteps);
                }
            }

            return steps;
        }

        /// <summary>
        /// Returns the distinct set of attributes that was sent in a given organization request
        /// </summary>
        /// <returns></returns>
        internal static string[] GetOrganizationRequestFilteringAttributes(OrganizationRequest request)
        {
            var target = GetTargetForRequest(request);
            if (target != null)
            {
                var entity = target as Entity;
                if (entity != null)
                {
                    return entity.Attributes.Keys.ToArray();
                }
            }
            else
            {
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
            }

            return new string[] { };
        }

        internal static string GetOrganizationRequestEntityLogicalName(OrganizationRequest request)
        {
            var target = GetTargetForRequest(request);
            if (target != null)
            {
                var entity = target as Entity;
                if (entity != null)
                {
                    return entity.LogicalName;
                }

                var entityReference = target as EntityReference;
                if (entityReference != null)
                {
                    return entityReference.LogicalName;
                }
            }
            else
            {
                var targets = GetTargetsForRequest(request);
                var entityCollection = targets as EntityCollection;
                if (entityCollection != null)
                {
                    return entityCollection.EntityName;
                }
            }
            
            return null;
        }
        
        private static List<PluginStepDefinition> GetStepsForStage(IXrmFakedContext context,
                                                                            PipelineStageExecutionParameters parameters)
        {
            int? entityTypeCode = null;
            
            var requestDistinctAttributes = GetOrganizationRequestFilteringAttributes(parameters.Request);
            string entityLogicalName = GetOrganizationRequestEntityLogicalName(parameters.Request);
            if (entityLogicalName != null)
            {
                var entityType = context.FindReflectedType(entityLogicalName);
                if (entityType != null)
                {
                    var entity = Activator.CreateInstance(entityType);
                    entityTypeCode = (int?)entity.GetType().GetField("EntityTypeCode")?.GetValue(entity);
                }
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
                        .Where(ps => !ps.FilteringAttributes.Any() || ps.FilteringAttributes.Any(attr => requestDistinctAttributes.Contains(attr))).ToList();
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