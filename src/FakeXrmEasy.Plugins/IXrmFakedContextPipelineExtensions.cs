using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Plugins;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using FakeXrmEasy.Plugins;
using FakeXrmEasy.Extensions;
using FakeXrmEasy.Plugins.PluginImages;
using FakeXrmEasy.Middleware.Pipeline;
using FakeXrmEasy.Plugins.Audit;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions;
using FakeXrmEasy.Plugins.PluginSteps.PluginStepRegistrationFieldNames;
using FakeXrmEasy.Plugins.PluginSteps.Extensions;
using FakeXrmEasy.Plugins.Definitions;
using FakeXrmEasy.Plugins.Pipeline.PipelineTypes;
using FakeXrmEasy.Plugins.PluginInstances;

namespace FakeXrmEasy.Pipeline
{
    /// <summary>
    /// Extension methods to register plugin steps
    /// </summary>
    public static class IXrmFakedContextPipelineExtensions
    {
        /// <summary>
        /// Registers a new plugin againts the specified plugin with the plugin step definition details provided
        /// When using this method the plugin class specified in the method signature will be used instead of the assembly and plugin
        /// types provided in the plugin step definition parameter.
        /// 
        /// All the other remaining settings in the plugin definition parameter will be used for registration.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="pluginStepDefinition">Info about the details of the plugin registration</param>
        /// <returns></returns>
        public static Guid RegisterPluginStep<TPlugin>(this IXrmFakedContext context,
                                            IPluginStepDefinition pluginStepDefinition)
            where TPlugin : IPlugin
        {
            return PluginStepRegistrationManager.RegisterPluginStepInternal<TPlugin>(context, pluginStepDefinition);
        }

        /// <summary>
        /// Registers a plugin step using an entity logical name, ideal for late bound entities
        /// </summary>
        /// <typeparam name="TPlugin">The plugin assembly to register</typeparam>
        /// <param name="context">The IXrmFakedContext where the registration will be stored </param>
        /// <param name="entityLogicalName">The logical name of the entity</param>
        /// <param name="message">The message that will trigger the plugin (i.e. request name)</param>
        /// <param name="stage">The stage in which the plugin will trigger</param>
        /// <param name="mode">The execution mode (async or sync)</param>
        /// <param name="rank">If multiple plugins are registered for the same message, rank defines the order in which they'll be executed</param>
        /// <param name="filteringAttributes">Any filtering attributes (optional)</param>
        /// <param name="registeredImages">Any pre or post images (optional)</param>
        /// <returns></returns>
        [Obsolete("This method is deprecated, please start using the new RegisterPluginStep method that takes an IPluginStepDefinition as a parameter")]
        public static Guid RegisterPluginStep<TPlugin>(this IXrmFakedContext context,
                                                        string entityLogicalName,
                                                        string message,
                                                        ProcessingStepStage stage = ProcessingStepStage.Postoperation,
                                                        ProcessingStepMode mode = ProcessingStepMode.Synchronous,
                                                        int rank = 1,
                                                        string[] filteringAttributes = null,
                                                        IEnumerable<PluginImageDefinition> registeredImages = null)
            where TPlugin : IPlugin
        {
            var pluginStepDefinition = new PluginStepDefinition()
            {
                MessageName = message,
                Stage = stage,
                Mode = mode,
                Rank = rank,
                FilteringAttributes = filteringAttributes,
                EntityTypeCode = null,
                EntityLogicalName = entityLogicalName,
                ImagesDefinitions = registeredImages
            };

            return PluginStepRegistrationManager.RegisterPluginStepInternal<TPlugin>(context, pluginStepDefinition);
        }


        /// <summary>
        /// Registers the <typeparamref name="TPlugin"/> as a SDK Message Processing Step for the Entity <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TPlugin">The plugin to register the step for.</typeparam>
        /// <typeparam name="TEntity">The entity to filter this step for.</typeparam>
        /// <param name="context">The faked context to register this plugin against</param>
        /// <param name="message">The message that should trigger the execution of plugin.</param>
        /// <param name="stage">The stage when the plugin should be executed.</param>
        /// <param name="mode">The mode in which the plugin should be executed.</param>
        /// <param name="rank">The order in which this plugin should be executed in comparison to other plugins registered with the same <paramref name="message"/> and <paramref name="stage"/>.</param>
        /// <param name="filteringAttributes">When not one of these attributes is present in the execution context, the execution of the plugin is prevented.</param>
        /// <param name="registeredImages">Optional, the any images to register against this plugin step</param>
        [Obsolete("This method is deprecated, please start using the new RegisterPluginStep method that takes an IPluginStepDefinition as a parameter")]
        public static Guid RegisterPluginStep<TPlugin, TEntity>(this IXrmFakedContext context,
                                                                    string message,
                                                                    ProcessingStepStage stage = ProcessingStepStage.Postoperation,
                                                                    ProcessingStepMode mode = ProcessingStepMode.Synchronous,
                                                                    int rank = 1,
                                                                    string[] filteringAttributes = null,
                                                                    IEnumerable<PluginImageDefinition> registeredImages = null)
            where TPlugin : IPlugin
            where TEntity : Entity, new()
        {
            var entity = new TEntity();
            var entityType = entity.GetType();
            if (entityType.IsSubclassOf(typeof(Entity)))
            {
                var pluginStepDefinition = new PluginStepDefinition()
                {
                    MessageName = message,
                    Stage = stage,
                    Mode = mode,
                    Rank = rank,
                    FilteringAttributes = filteringAttributes,
                    EntityLogicalName = entity.LogicalName,
                    ImagesDefinitions = registeredImages
                };

                return PluginStepRegistrationManager.RegisterPluginStepInternal<TPlugin>(context, pluginStepDefinition);
            }
            else
            {
                throw new InvalidRegistrationMethodForLateBoundException();
            }
        }

        /// <summary>
        /// Registers the <typeparamref name="TPlugin"/> as a SDK Message Processing Step.
        /// </summary>
        /// <typeparam name="TPlugin">The plugin to register the step for.</typeparam>
        /// <param name="context">The faked context to register this plugin against</param>
        /// <param name="message">The message that should trigger the execution of plugin.</param>
        /// <param name="stage">The stage when the plugin should be executed.</param>
        /// <param name="mode">The mode in which the plugin should be executed.</param>
        /// <param name="rank">The order in which this plugin should be executed in comparison to other plugins registered with the same <paramref name="message"/> and <paramref name="stage"/>.</param>
        /// <param name="filteringAttributes">When not one of these attributes is present in the execution context, the execution of the plugin is prevented.</param>
        /// <param name="primaryEntityTypeCode">The entity type code to filter this step for.</param>
        /// <param name="registeredImages">Optional, the any images to register against this plugin step</param>
        [Obsolete("This method is deprecated, please start using the new RegisterPluginStep method that takes an IPluginStepDefinition as a parameter")]
        public static Guid RegisterPluginStep<TPlugin>(this IXrmFakedContext context,
                                                        string message,
                                                        ProcessingStepStage stage = ProcessingStepStage.Postoperation,
                                                        ProcessingStepMode mode = ProcessingStepMode.Synchronous,
                                                        int rank = 1,
                                                        string[] filteringAttributes = null,
                                                        int? primaryEntityTypeCode = null,
                                                        IEnumerable<PluginImageDefinition> registeredImages = null)
            where TPlugin : IPlugin
        {
            var pluginStepDefinition = new PluginStepDefinition()
            {
                MessageName = message,
                Stage = stage,
                Mode = mode,
                Rank = rank,
                FilteringAttributes = filteringAttributes,
                EntityTypeCode = primaryEntityTypeCode,
                EntityLogicalName = null,
                ImagesDefinitions = registeredImages
            };

            return PluginStepRegistrationManager.RegisterPluginStepInternal<TPlugin>(context, pluginStepDefinition);
        }

        internal static void ExecutePipelineStage(this IXrmFakedContext context, PipelineStageExecutionParameters parameters)
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
                    context.ExecutePipelinePlugins(plugins, parameters.Request, parameters.PreEntitySnapshot, parameters.PostEntitySnapshot, parameters.Response);
                }

                var entityReferenceTarget = target as EntityReference;
                if (entityReferenceTarget != null)
                {
                    var entityType = context.FindReflectedType(entityReferenceTarget.LogicalName);
                    if (entityType == null)
                    {
                        return;
                    }
                    context.ExecutePipelinePlugins(plugins, parameters.Request, parameters.PreEntitySnapshot, parameters.PostEntitySnapshot, parameters.Response);
                }
            }
            else
            {
                context.ExecutePipelinePlugins(plugins, parameters.Request, null, null, parameters.Response);
            }
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
                context.AddPluginStepAuditDetails(pluginMethod, pluginContext, pluginStepDefinition);
            }
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
        private static void ExecutePipelinePlugins(this IXrmFakedContext context,
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

        private static void AddPluginStepAuditDetails(this IXrmFakedContext context,
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

        


    }
}
