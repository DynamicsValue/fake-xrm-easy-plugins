using System;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Middleware;
using Microsoft.Xrm.Sdk;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.Audit;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.Middleware.Pipeline.Exceptions;
using FakeXrmEasy.Plugins.PluginInstances;

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
                
                context.SetProperty<IPluginInstancesRepository>(new PluginInstancesRepository());
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
                    PluginStepRegistrationManager.RegisterPluginStepInternal(context, pluginType, stepDefinition);
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
                        return PipelineProcessor.ProcessPipelineRequest(request, context, next);
                    }

                    return next.Invoke(context, request);
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
    }
}
