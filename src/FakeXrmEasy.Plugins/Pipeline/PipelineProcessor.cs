using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Middleware;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Extensions;
using FakeXrmEasy.Middleware.Pipeline;
using FakeXrmEasy.Pipeline.Scope;
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
            var pipelineParameters = PipelineStageExecutionParameters.FromOrganizationRequest(request);

            PopulatePreEntityImagesForRequest(context, pipelineParameters);

            ProcessPreValidation(context, pipelineParameters);
            ProcessPreOperation(context, pipelineParameters);

            var response = next.Invoke(context, pipelineParameters.Request);

            pipelineParameters.Response = response;
            PopulatePostEntityImagesForRequest(context, pipelineParameters);
            ProcessPostOperation(context, pipelineParameters);
            
            return response;
        }
        
        private static void ProcessPreValidation(IXrmFakedContext context, 
            PipelineStageExecutionParameters pipelineParameters)
        {
            pipelineParameters.Stage = ProcessingStepStage.Prevalidation;
            pipelineParameters.Mode = ProcessingStepMode.Synchronous;
            ExecutePipelineStage(context, pipelineParameters);
        }
        
        private static void ProcessPreOperation(IXrmFakedContext context, 
            PipelineStageExecutionParameters pipelineParameters) 
        {  
            pipelineParameters.Stage = ProcessingStepStage.Preoperation;
            pipelineParameters.Mode = ProcessingStepMode.Synchronous;
            ExecutePipelineStage(context, pipelineParameters);
        }

        private static void ProcessPostOperation(IXrmFakedContext context,
            PipelineStageExecutionParameters pipelineParameters) 
        {
            pipelineParameters.Stage = ProcessingStepStage.Postoperation;
            pipelineParameters.Mode = ProcessingStepMode.Synchronous;
            ExecutePipelineStage(context, pipelineParameters);

            pipelineParameters.Mode = ProcessingStepMode.Asynchronous;
            ExecutePipelineStage(context, pipelineParameters);
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
            IPluginStepDefinition pluginStepDefinition, 
            EventPipelineScope scope)
        {
            if (pluginStepDefinition.PluginInstance != null)
            {
                pluginMethod.Invoke(null, new object[] { scope, pluginStepDefinition.PluginInstance });
            }
            else if (pluginStepDefinition.Configurations != null)
            {
                pluginMethod.Invoke(null, new object[] {  
                    scope, 
                    pluginStepDefinition.Configurations.UnsecureConfig,
                    pluginStepDefinition.Configurations.SecureConfig
                });
            }
            else
            {
                pluginMethod.Invoke(null, new object[] { scope });
            }
        }
        
        private static MethodInfo GetPluginMethod(PluginStepDefinition pluginStepDefinition)
        {
            var assembly = AppDomain.CurrentDomain.Load(pluginStepDefinition.AssemblyName);
            var pluginType = assembly.GetType(pluginStepDefinition.PluginType);

            MethodInfo methodInfo = null;
            if (pluginStepDefinition.PluginInstance != null)
            {
                methodInfo = typeof(EventPipelineScopePluginExecutor).GetMethod("ExecutePluginWith", new[] { typeof(EventPipelineScope), typeof(IPlugin) });
                return methodInfo;
            }
            
            if (pluginStepDefinition.Configurations != null)
            {
                methodInfo = typeof(EventPipelineScopePluginExecutor).GetMethod("ExecutePluginWithConfigurations", new[] { typeof(EventPipelineScope), typeof(string), typeof(string) });
            }
            else
            {
                methodInfo = typeof(EventPipelineScopePluginExecutor).GetMethod("ExecutePluginWith", new[] { typeof(EventPipelineScope) });
            }
            
            return methodInfo.MakeGenericMethod(pluginType);
        }
        
        /// <summary>
        /// Executes all the relevant plugin steps for the current request
        /// </summary>
        /// <param name="context"></param>
        /// <param name="pluginSteps"></param>
        /// <param name="parameters"></param>
        /// <exception cref="Exception"></exception>
        private static void ExecutePipelinePlugins(IXrmFakedContext context,
                                                    IEnumerable<PluginStepDefinition> pluginSteps,
                                                    PipelineStageExecutionParameters parameters)
        {
            var isAuditEnabled = context.GetProperty<PipelineOptions>().UsePluginStepAudit;
            
            foreach (var pluginStep in pluginSteps)
            {
                IEnumerable<Entity> preImageDefinitions = null;
                if (parameters.PreEntitySnapshot != null)
                {
                    preImageDefinitions = RegisteredPluginStepsRetriever.GetPluginImageDefinitions(context, pluginStep.Id, ProcessingStepImageType.PreImage);
                }

                IEnumerable<Entity> postImageDefinitions = null;
                if (parameters.PostEntitySnapshot != null)
                {
                    postImageDefinitions = RegisteredPluginStepsRetriever.GetPluginImageDefinitions(context, pluginStep.Id, ProcessingStepImageType.PostImage);
                }

                var pluginContext = context.GetDefaultPluginContext();
                pluginContext.Mode = (int)pluginStep.Mode;
                pluginContext.Stage = (int)pluginStep.Stage;
                pluginContext.MessageName = pluginStep.MessageName;
                pluginContext.InputParameters = parameters.Request.Parameters;
                pluginContext.OutputParameters = parameters.Response != null ? parameters.Response.Results : new ParameterCollection();
                pluginContext.PreEntityImages = GetEntityImageCollection(preImageDefinitions, parameters.PreEntitySnapshot);
                pluginContext.PostEntityImages = GetEntityImageCollection(postImageDefinitions, parameters.PostEntitySnapshot);
                pluginContext.Depth = 1;
                
                if (parameters.Scope != null)
                {
                    pluginContext.Depth = parameters.Scope.PluginContext.Depth + 1;

                    var pipelineOptions = context.GetProperty<PipelineOptions>();
                    if (pluginContext.Depth > pipelineOptions.MaxDepth)
                    {
                        throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.SdkCorrelationTokenDepthTooHigh,
                            "This workflow job was canceled because the workflow that started it included an infinite loop. Correct the workflow logic and try again. For information about workflow logic, see Help");
                    }
                    pluginContext.ParentContext = parameters.Scope.PluginContext;
                }

                var newScope = new EventPipelineScope()
                {
                    PluginContext = pluginContext
                };
                
                var pipelineOrganizationService =
                    PipelineOrganizationServiceFactory.New(context.GetOrganizationService(),
                        newScope);

                newScope.PluginContextProperties = new XrmFakedPluginContextProperties(context,
                    pipelineOrganizationService, context.GetTracingService());
                
                ExecutePlugin(context, pluginStep, newScope, isAuditEnabled);
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
                PluginStepDefinition = pluginStep,
                PluginContext = pluginContext
            };

            if (pluginContext.InputParameters.ContainsKey("Target"))
            {
                var target = pluginContext.InputParameters["Target"];
                var targetEntity = target as Entity;
                if (targetEntity != null)
                    pluginStepAuditDetails.TargetEntity = targetEntity;

                var targetEntityRef = target as EntityReference;
                if (targetEntityRef != null)
                    pluginStepAuditDetails.TargetEntityReference = targetEntityRef;
            }

            var pluginStepAudit = context.GetProperty<IPluginStepAudit>() as PluginStepAudit;
            pluginStepAudit.Add(pluginStepAuditDetails);
        }
        
        private static void ExecutePlugin(IXrmFakedContext context,
            PluginStepDefinition pluginStepDefinition,
            EventPipelineScope scope,
            bool isAuditEnabled)
        {
            var pluginMethod = GetPluginMethod(pluginStepDefinition);
            
            try
            {
                InvokePluginMethod(pluginMethod, pluginStepDefinition, scope);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
            
            if (isAuditEnabled)
            {
                AddPluginStepAuditDetails(context, pluginMethod, scope.PluginContext, pluginStepDefinition);
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
            
            ExecutePipelinePlugins(context, plugins, parameters);
        }

        internal static void PopulatePreEntityImagesForRequest(IXrmFakedContext context,
            PipelineStageExecutionParameters pipelineParameters)
        {
            if (!PreImage.IsAvailableFor(pipelineParameters.Request.GetType()))
            {
                return;
            }

            if (pipelineParameters.Request.IsBulkOperation())
            {
                pipelineParameters.PreEntitySnapshotCollection = GetPreImageEntityCollectionForRequest(context, pipelineParameters.Request)
                                                                        .Entities
                                                                        .ToList();
            }
            else
            {
                pipelineParameters.PreEntitySnapshot = GetPreImageEntityForRequest(context, pipelineParameters.Request);
            }    
        }
        
        internal static void PopulatePostEntityImagesForRequest(IXrmFakedContext context,
            PipelineStageExecutionParameters pipelineParameters)
        {
            if (!PostImage.IsAvailableFor(pipelineParameters.Request.GetType(), ProcessingStepStage.Postoperation))
            {
                return;
            }

            if (pipelineParameters.Request.IsBulkOperation())
            {
                pipelineParameters.PostEntitySnapshotCollection = GetPostImageEntityCollectionForRequest(context, pipelineParameters.Request)
                    .Entities
                    .ToList();
            }
            else
            {
                pipelineParameters.PostEntitySnapshot = GetPostImageEntityForRequest(context, pipelineParameters.Request);
            }    
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

            var targetEntity = target as Entity;
            var targetEntityRef = target as EntityReference; 
            if (targetEntity != null)
            {
                logicalName = targetEntity.LogicalName;
                id = targetEntity.Id;
            }
            else if (targetEntityRef != null)
            {
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

        internal static Entity GetPostImageEntityForTarget(IXrmFakedContext context, object target)
        {
            string logicalName = "";
            Guid id = Guid.Empty;

            var targetEntity = target as Entity;
            var targetEntityRef = target as EntityReference; 
            
            if (targetEntity != null)
            {
                logicalName = targetEntity.LogicalName;
                id = targetEntity.Id;

                if (id == Guid.Empty)
                {
                    return targetEntity;
                }
            }
            else if (targetEntityRef != null)
            {
                logicalName = targetEntityRef.LogicalName;
                id = targetEntityRef.Id;
            }

            var postImage = context.GetEntityById(logicalName, id);
            if (targetEntity != null)
            {
                postImage = postImage.ReplaceAttributesWith(targetEntity);
            }

            return postImage;
        }

        internal static Entity GetPostImageEntityForRequest(IXrmFakedContext context, OrganizationRequest request)
        {
            var target = RegisteredPluginStepsRetriever.GetTargetForRequest(request);
            if (target == null)
            {
                return null;
            }

            return GetPostImageEntityForTarget(context, target);

        }

        internal static EntityCollection GetPostImageEntityCollectionForRequest(IXrmFakedContext context, OrganizationRequest request)
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
                    entities.Add(GetPostImageEntityForTarget(context, target));
                }
            }
            
            var entityReferenceCollection = targets as EntityReferenceCollection;
            if (entityReferenceCollection != null)
            {
                foreach (var target in entityReferenceCollection)
                {
                    entities.Add(GetPostImageEntityForTarget(context, target));
                }
            }

            return new EntityCollection(entities);
        }
    }
}