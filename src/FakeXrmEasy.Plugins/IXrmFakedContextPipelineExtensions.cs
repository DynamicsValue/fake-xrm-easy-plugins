using System;
using System.Collections.Generic;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using Microsoft.Xrm.Sdk;
using FakeXrmEasy.Plugins.PluginImages;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions;
using FakeXrmEasy.Plugins.Definitions;

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
    }
}
