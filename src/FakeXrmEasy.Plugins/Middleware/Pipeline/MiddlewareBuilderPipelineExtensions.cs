using System;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Middleware;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using Microsoft.Xrm.Sdk;

using FakeXrmEasy.Pipeline;

namespace FakeXrmEasy.Middleware.Pipeline
{
    public static class MiddlewareBuilderPipelineExtensions
    {
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

        public static IMiddlewareBuilder UsePipelineSimulation(this IMiddlewareBuilder builder) 
        {

            Func<OrganizationRequestDelegate, OrganizationRequestDelegate> middleware = next => {

                return (IXrmFakedContext context, OrganizationRequest request) => {
                    
                    if(CanHandleRequest(context, request)) 
                    {
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
