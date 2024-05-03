using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Extensions;
using FakeXrmEasy.Middleware.Pipeline;
using FakeXrmEasy.Plugins;
using FakeXrmEasy.Plugins.Audit;
using FakeXrmEasy.Plugins.Definitions;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.PluginSteps.PluginStepRegistrationFieldNames;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Pipeline
{
    /// <summary>
    /// Class in charge of the simulated event execution pipeline
    /// </summary>
    internal static class PipelineProcessor
    {
        /// <summary>
        /// Gets an entity image collection for each registered entity image and the current entity record
        /// </summary>
        /// <param name="imageDefinitions"></param>
        /// <param name="values"></param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Executes all the relevant plugin steps for the current request
        /// </summary>
        /// <param name="context"></param>
        /// <param name="pluginSteps"></param>
        /// <param name="organizationRequest"></param>
        /// <param name="previousValues"></param>
        /// <param name="resultingAttributes"></param>
        /// <param name="organizationResponse">The organization response that triggered this plugin execution</param>
        private static void ExecutePipelinePlugins(IXrmFakedContext context,
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
                    preImageDefinitions = RegisteredPluginStepsRetriever.GetPluginImageDefinitions(context, pluginStep.Id, ProcessingStepImageType.PreImage);
                }

                IEnumerable<Entity> postImageDefinitions = null;
                if (resultingAttributes != null)
                {
                    postImageDefinitions = RegisteredPluginStepsRetriever.GetPluginImageDefinitions(context, pluginStep.Id, ProcessingStepImageType.PostImage);
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
        
        private static void AddPluginStepAuditDetails(IXrmFakedContext context,
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
                AddPluginStepAuditDetails(context, pluginMethod, pluginContext, pluginStepDefinition);
            }
        }
        
        internal static void ExecutePipelineStage(IXrmFakedContext context, PipelineStageExecutionParameters parameters)
        {
            var plugins = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(context, parameters.RequestName, parameters.Stage, parameters.Mode, parameters.Request);
            if (plugins == null)
                return;

            var target = RegisteredPluginStepsRetriever.GetTargetForRequest(parameters.Request);

            if (target != null)
            {
                var entityTarget = target as Entity;
                if (entityTarget != null)
                {
                    ExecutePipelinePlugins(context, plugins, parameters.Request, parameters.PreEntitySnapshot, parameters.PostEntitySnapshot, parameters.Response);
                }

                var entityReferenceTarget = target as EntityReference;
                if (entityReferenceTarget != null)
                {
                    /*
                    var entityType = context.FindReflectedType(entityReferenceTarget.LogicalName);
                    if (entityType == null)
                    {
                        return;
                    }
                    */
                    ExecutePipelinePlugins(context, plugins, parameters.Request, parameters.PreEntitySnapshot, parameters.PostEntitySnapshot, parameters.Response);
                }
            }
            else
            {
                ExecutePipelinePlugins(context, plugins, parameters.Request, null, null, parameters.Response);
            }
        }
    }
}