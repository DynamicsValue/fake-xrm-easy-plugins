using System;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Middleware;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using Microsoft.Xrm.Sdk;

using FakeXrmEasy.Pipeline;

namespace FakeXrmEasy.Middleware.Pipeline
{
    /// <summary>
    /// Provides extensions for plugin pipeline simulation
    /// </summary>
    public static class MiddlewareBuilderPipelineExtensions
    {
        /// <summary>
        /// Enables Pipeline Simulation in middleware
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMiddlewareBuilder AddPipelineSimulation(this IMiddlewareBuilder builder) 
        {
            builder.Add(context => {
                var pipelineOptions = new PipelineOptions()
                {
                    UsePipelineSimulation = true
                };

                context.SetProperty(pipelineOptions);
            });

            return builder;
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
                        ProcessPreValidation(context, request);
                        ProcessPreOperation(context, request);
                        var response = next.Invoke(context, request);
                        ProcessPostOperation(context, request);
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

        private static void ProcessPreValidation(IXrmFakedContext context, OrganizationRequest request)
        {
            context.ExecutePipelineStage(request.RequestName, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous, request);
        }

        private static void ProcessPreOperation(IXrmFakedContext context, OrganizationRequest request) 
        {
            context.ExecutePipelineStage(request.RequestName, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous, request);
        }

        private static void ProcessPostOperation(IXrmFakedContext context, OrganizationRequest request) 
        {
            context.ExecutePipelineStage(request.RequestName, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous, request);
            context.ExecutePipelineStage(request.RequestName, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous, request);
        }

    }
}
