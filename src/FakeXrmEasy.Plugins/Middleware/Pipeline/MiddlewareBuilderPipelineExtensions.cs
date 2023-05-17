using System;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Middleware;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using Microsoft.Xrm.Sdk;

using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Extensions;
using FakeXrmEasy.Plugins.PluginImages;
using FakeXrmEasy.Plugins.Audit;
using FakeXrmEasy.Plugins.PluginSteps;
using System.Linq;
using FakeXrmEasy.Plugins.Middleware.Pipeline.Exceptions;
using FakeXrmEasy.Plugins.Extensions;

namespace FakeXrmEasy.Middleware.Pipeline
{
    /// <summary>
    /// Provides extensions for plugin pipeline simulation
    /// </summary>
    public static class MiddlewareBuilderPipelineExtensions
    {
        /// <summary>
        /// Enables Pipeline Simulation in middleware with default options
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMiddlewareBuilder AddPipelineSimulation(this IMiddlewareBuilder builder) 
        {
            return builder.AddPipelineSimulation(new PipelineOptions());
        }

        /// <summary>
        /// Enables Piepeline Simulation in middleware with custom options
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options">The custom options</param>
        /// <returns></returns>
        public static IMiddlewareBuilder AddPipelineSimulation(this IMiddlewareBuilder builder, PipelineOptions options)
        {
            builder.Add(context => {
                context.SetProperty(options);

                if(options.UsePluginStepAudit)
                {
                    context.SetProperty<IPluginStepAudit>(new PluginStepAudit());
                }

                if(options.UsePluginStepRegistrationValidation)
                {
                    context.SetProperty<IPluginStepValidator>(new PluginStepValidator());
                }

                if(options.UseAutomaticPluginStepRegistration)
                {
                    DiscoverAndRegisterPluginSteps(context, options);
                }
            });

            return builder;
        }

        private static void DiscoverAndRegisterPluginSteps(IXrmFakedContext context, PipelineOptions options)
        {
            if(options.CustomPluginStepDiscoveryFunction == null)
            {
                throw new CustomDiscoveryFunctionMissingException();
            }

            DiscoverAndRegisterCustomPluginSteps(context, options);
        }

        private static void DiscoverAndRegisterCustomPluginSteps(IXrmFakedContext context, PipelineOptions options)
        {
            foreach (var assembly in options.PluginAssemblies)
            {
                var pluginStepDefinitions = options.CustomPluginStepDiscoveryFunction.Invoke(assembly);
                foreach (var stepDefinition in pluginStepDefinitions)
                {
                    if(string.IsNullOrWhiteSpace(stepDefinition.PluginType))
                    {
                        throw new MissingPluginTypeInPluginStepDefinitionException();
                    }
                    var pluginType = assembly.GetType(stepDefinition.PluginType);
                    context.RegisterPluginStepInternal(pluginType, stepDefinition);
                }
            }
        }

        /// <summary>
        /// Inserts pipeline simulation into the middleware. When using pipeline simulation, this method should be called before all the other Use*() methods
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMiddlewareBuilder UsePipelineSimulation(this IMiddlewareBuilder builder) 
        {
            Func<OrganizationRequestDelegate, OrganizationRequestDelegate> middleware = next => {

                return (IXrmFakedContext context, OrganizationRequest request) => {
                    
                    if(CanHandleRequest(context, request)) 
                    {
                        var preImagePreValidation = PreImage.IsAvailableFor(request.GetType(), ProcessingStepStage.Prevalidation) ?
                                            GetPreImageEntityForRequest(context, request) : null;

                        var preImagePreOperation = PreImage.IsAvailableFor(request.GetType(), ProcessingStepStage.Preoperation) ?
                                            GetPreImageEntityForRequest(context, request) : null;

                        var preImagePostOperation = PreImage.IsAvailableFor(request.GetType(), ProcessingStepStage.Postoperation) ?
                                            GetPreImageEntityForRequest(context, request) : null;

                        ProcessPreValidation(context, request, preImagePreValidation);
                        ProcessPreOperation(context, request, preImagePreOperation);

                        var response = next.Invoke(context, request);

                        var postImagePostOperation = PostImage.IsAvailableFor(request.GetType(), ProcessingStepStage.Postoperation) ?
                                            GetPostImageEntityForRequest(context, request) : null;

                        ProcessPostOperation(context, request, response, preImagePostOperation, postImagePostOperation);
                        return response;
                    }
                    else 
                    {
                        return next.Invoke(context, request);
                    }
                };
            };
            
            builder.Use(middleware);
            return builder;
        }

        private static bool CanHandleRequest(IXrmFakedContext context, OrganizationRequest request) 
        {
            var pipelineOptions = context.GetProperty<PipelineOptions>();
            return pipelineOptions?.UsePipelineSimulation == true;
        }

        private static void ProcessPreValidation(IXrmFakedContext context, 
                                OrganizationRequest request, 
                                Entity preEntity = null, 
                                Entity postEntity = null)
        {
            var pipelineParameters = new PipelineStageExecutionParameters()
            {
                RequestName = request.RequestName,
                Request = request,
                Stage = ProcessingStepStage.Prevalidation,
                Mode = ProcessingStepMode.Synchronous,
                PreEntitySnapshot = preEntity,
                PostEntitySnapshot = postEntity
            };

            context.ExecutePipelineStage(pipelineParameters);
        }

        private static void ProcessPreOperation(IXrmFakedContext context, 
                                        OrganizationRequest request, 
                                        Entity preEntity = null, 
                                        Entity postEntity = null) 
        {
            var pipelineParameters = new PipelineStageExecutionParameters()
            {
                RequestName = request.RequestName,
                Request = request,
                Stage = ProcessingStepStage.Preoperation,
                Mode = ProcessingStepMode.Synchronous,
                PreEntitySnapshot = preEntity,
                PostEntitySnapshot = postEntity
            };

            context.ExecutePipelineStage(pipelineParameters);
        }

        private static void ProcessPostOperation(IXrmFakedContext context, 
                                                    OrganizationRequest request, 
                                                    OrganizationResponse response,
                                                    Entity preEntity = null, 
                                                    Entity postEntity = null) 
        {
            var pipelineParameters = new PipelineStageExecutionParameters()
            {
                RequestName = request.RequestName,
                Request = request,
                Response = response,
                Stage = ProcessingStepStage.Postoperation,
                Mode = ProcessingStepMode.Synchronous,
                PreEntitySnapshot = preEntity,
                PostEntitySnapshot = postEntity
            };
            context.ExecutePipelineStage(pipelineParameters);

            pipelineParameters.Mode = ProcessingStepMode.Asynchronous;
            context.ExecutePipelineStage(pipelineParameters);
        }

        private static Entity GetPreImageEntityForRequest(IXrmFakedContext context, OrganizationRequest request)
        {
            var target = IXrmFakedContextPipelineExtensions.GetTargetForRequest(request);
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
            }

            else if (target is EntityReference)
            {
                var targetEntityRef = target as EntityReference;
                logicalName = targetEntityRef.LogicalName;
                id = targetEntityRef.Id;
            }

            return context.GetEntityById(logicalName, id);
        }

        private static Entity GetPostImageEntityForRequest(IXrmFakedContext context, OrganizationRequest request)
        {
            var target = IXrmFakedContextPipelineExtensions.GetTargetForRequest(request);
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
    }
}
