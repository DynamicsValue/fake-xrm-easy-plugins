using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Middleware;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Extensions;
using FakeXrmEasy.Middleware.Pipeline;
using FakeXrmEasy.Plugins;
using FakeXrmEasy.Plugins.Audit;
using FakeXrmEasy.Plugins.Definitions;
using FakeXrmEasy.Plugins.Extensions;
using FakeXrmEasy.Plugins.PluginImages;
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
        /// Processes the event execution pipeline for the current request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        internal static OrganizationResponse ProcessPipelineRequest(OrganizationRequest request, IXrmFakedContext context,
            OrganizationRequestDelegate next)
        {
            var preImagePostOperation = PreImage.IsAvailableFor(request.GetType(), ProcessingStepStage.Postoperation)
                ? GetPreImageEntityForRequest(context, request)
                : null;

            ProcessPreValidation(context, request);
            ProcessPreOperation(context, request);

            var response = next.Invoke(context, request);
            
            ProcessPostOperation(context, request, response, preImagePostOperation);
            return response;
        }
        
        private static void ProcessPreValidation(IXrmFakedContext context, 
            OrganizationRequest request)
        {
            var preImagePreValidation = PreImage.IsAvailableFor(request.GetType(), ProcessingStepStage.Prevalidation)
                ? GetPreImageEntityForRequest(context, request)
                : null;
            
            var pipelineParameters = new PipelineStageExecutionParameters()
            {
                Request = request,
                Stage = ProcessingStepStage.Prevalidation,
                Mode = ProcessingStepMode.Synchronous,
                PreEntitySnapshot = preImagePreValidation,
                PostEntitySnapshot = null
            };

            PipelineProcessor.ExecutePipelineStage(context, pipelineParameters);
        }
        
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
                Mode = (ProcessingStepMode)pluginContext.Mode,
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
        
        /// <summary>
        /// Retrieves any necessary plugins for the current stage and mode and executes them with the given request
        /// </summary>
        /// <param name="context"></param>
        /// <param name="parameters"></param>
        internal static void ExecutePipelineStage(IXrmFakedContext context, PipelineStageExecutionParameters parameters)
        {
            var plugins = RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(context, parameters);
            if (plugins == null)
                return;

            ExecutePipelinePlugins(context, plugins, parameters.Request, parameters.PreEntitySnapshot, parameters.PostEntitySnapshot, parameters.Response);
        }

        internal static Entity GetPreImageEntityForRequest(IXrmFakedContext context, OrganizationRequest request)
        {
            var target = RegisteredPluginStepsRetriever.GetTargetForRequest(request);
            if (target == null)
            {
                return null;
            }

            return GetPreImageEntityForTarget(context, target);
        }

        private static Entity GetPreImageEntityForTarget(IXrmFakedContext context, object target)
        {
            string logicalName = "";
            Guid id = Guid.Empty;

            if (target is Entity)
            {
                var targetEntity = target as Entity;
                logicalName = targetEntity.LogicalName;
                id = targetEntity.Id;
            }

            else if (target is EntityReference)
            {
                var targetEntityRef = target as EntityReference;
                logicalName = targetEntityRef.LogicalName;
                id = targetEntityRef.Id;
            }

            return context.GetEntityById(logicalName, id);
        }
        
        internal static EntityCollection GetPreImageEntityCollectionForRequest(IXrmFakedContext context, OrganizationRequest request)
        {
            var targets = RegisteredPluginStepsRetriever.GetTargetsForRequest(request);
            if (targets == null)
            {
                return null;
            }
            
            var entities = new List<Entity>();

            var entityCollection = targets as EntityCollection;
            if (entityCollection != null)
            {
                foreach (var target in entityCollection.Entities)
                {
                    entities.Add(GetPreImageEntityForTarget(context, target));
                }
            }
            
            var entityReferenceCollection = targets as EntityReferenceCollection;
            if (entityReferenceCollection != null)
            {
                foreach (var target in entityReferenceCollection)
                {
                    entities.Add(GetPreImageEntityForTarget(context, target));
                }
            }

            return new EntityCollection(entities);
        }

        private static Entity GetPostImageEntityForRequest(IXrmFakedContext context, OrganizationRequest request)
        {
            var target = RegisteredPluginStepsRetriever.GetTargetForRequest(request);
            if (target == null)
            {
                return null;
            }

            string logicalName = "";
            Guid id = Guid.Empty;

            if (target is Entity)
            {
                var targetEntity = target as Entity;
                logicalName = targetEntity.LogicalName;
                id = targetEntity.Id;

                if (id == Guid.Empty)
                {
                    return targetEntity;
                }
            }

            else if (target is EntityReference)
            {
                var targetEntityRef = target as EntityReference;
                logicalName = targetEntityRef.LogicalName;
                id = targetEntityRef.Id;
            }

            var postImage = context.GetEntityById(logicalName, id);
            if (target is Entity)
            {
                postImage = postImage.ReplaceAttributesWith(target as Entity);
            }

            return postImage;
        }
        
        private static void ProcessPreOperation(IXrmFakedContext context, 
            OrganizationRequest request) 
        {  
            var preImagePreOperation = PreImage.IsAvailableFor(request.GetType(), ProcessingStepStage.Preoperation)
                ? GetPreImageEntityForRequest(context, request)
                : null;
            
            var pipelineParameters = new PipelineStageExecutionParameters()
            {
                Request = request,
                Stage = ProcessingStepStage.Preoperation,
                Mode = ProcessingStepMode.Synchronous,
                PreEntitySnapshot = preImagePreOperation,
                PostEntitySnapshot = null
            };

            ExecutePipelineStage(context, pipelineParameters);
        }

        private static void ProcessPostOperation(IXrmFakedContext context, 
            OrganizationRequest request, 
            OrganizationResponse response,
            Entity preEntity = null) 
        {
            var postImagePostOperation = PostImage.IsAvailableFor(request.GetType(), ProcessingStepStage.Postoperation)
                ? GetPostImageEntityForRequest(context, request)
                : null;
            
            var pipelineParameters = new PipelineStageExecutionParameters()
            {
                Request = request,
                Response = response,
                Stage = ProcessingStepStage.Postoperation,
                Mode = ProcessingStepMode.Synchronous,
                PreEntitySnapshot = preEntity,
                PostEntitySnapshot = postImagePostOperation
            };
            ExecutePipelineStage(context, pipelineParameters);

            pipelineParameters.Mode = ProcessingStepMode.Asynchronous;
            ExecutePipelineStage(context, pipelineParameters);
        }
    }
}