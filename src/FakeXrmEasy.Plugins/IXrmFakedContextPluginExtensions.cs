using System;
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk;
using FakeItEasy;
using FakeXrmEasy.Abstractions.Plugins;
using System.Linq;

namespace FakeXrmEasy.Plugins
{
    public static class IXrmFakedContextPluginExtensions
    {
        /// <summary>
        /// Returns a plugin context with default properties one can override
        /// </summary>
        /// <returns></returns>
        public static XrmFakedPluginExecutionContext GetDefaultPluginContext(this IXrmFakedContext context)
        {
            var userId = context.CallerProperties.CallerId?.Id ?? Guid.NewGuid();
            Guid businessUnitId = context.CallerProperties.BusinessUnitId?.Id ?? Guid.NewGuid();

            return new XrmFakedPluginExecutionContext
            {
                Depth = 1,
                IsExecutingOffline = false,
                MessageName = "Create",
                UserId = userId,
                BusinessUnitId = businessUnitId,
                InitiatingUserId = userId,
                InputParameters = new ParameterCollection(),
                OutputParameters = new ParameterCollection(),
                SharedVariables = new ParameterCollection(),
                PreEntityImages = new EntityImageCollection(),
                PostEntityImages = new EntityImageCollection(),
                IsolationMode = 1
            };
        }

        public static IXrmFakedPluginContextProperties GetPluginContextProperties(this IXrmFakedContext context) 
        {
            if(context.PluginContextProperties == null) 
            {
                context.PluginContextProperties = new XrmFakedPluginContextProperties(context.GetOrganizationService());
            }
            return context.PluginContextProperties;
        }

        /// <summary>
        /// Executes a plugin passing a custom context. This is useful whenever we need to mock more complex plugin contexts (ex: passing MessageName, plugin Depth, InitiatingUserId etc...)
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static IPlugin ExecutePluginWith(this IXrmFakedContext context, XrmFakedPluginExecutionContext plugCtx, IPlugin instance)
        {
            var fakedServiceProvider = context.GetPluginContextProperties().GetServiceProvider(plugCtx);

            var fakedPlugin = A.Fake<IPlugin>();
            A.CallTo(() => fakedPlugin.Execute(A<IServiceProvider>._))
                .Invokes((IServiceProvider provider) =>
                {
                    var plugin = instance;
                    plugin.Execute(fakedServiceProvider);
                });

            fakedPlugin.Execute(fakedServiceProvider);
            return fakedPlugin;
        }

        /// <summary>
        /// Executes a plugin passing a custom context. This is useful whenever we need to mock more complex plugin contexts (ex: passing MessageName, plugin Depth, InitiatingUserId etc...)
        /// </summary>
        /// <typeparam name="T">Must be a plugin</typeparam>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static IPlugin ExecutePluginWith<T>(this IXrmFakedContext context, XrmFakedPluginExecutionContext ctx = null)
            where T : IPlugin, new()
        {
            if (ctx == null)
            {
                ctx = context.GetDefaultPluginContext();
            }

            return context.ExecutePluginWith(ctx, new T());
        }


        public static IPlugin ExecutePluginWith<T>(this IXrmFakedContext context, ParameterCollection inputParameters, ParameterCollection outputParameters, EntityImageCollection preEntityImages, EntityImageCollection postEntityImages)
            where T : IPlugin, new()
        {
            var ctx = context.GetDefaultPluginContext();
            ctx.InputParameters.AddRange(inputParameters);
            ctx.OutputParameters.AddRange(outputParameters);
            ctx.PreEntityImages.AddRange(preEntityImages);
            ctx.PostEntityImages.AddRange(postEntityImages);

            var fakedServiceProvider = context.GetPluginContextProperties().GetServiceProvider(ctx);

            var fakedPlugin = A.Fake<IPlugin>();
            A.CallTo(() => fakedPlugin.Execute(A<IServiceProvider>._))
                .Invokes((IServiceProvider provider) =>
                {
                    var plugin = new T();
                    plugin.Execute(fakedServiceProvider);
                });

            fakedPlugin.Execute(fakedServiceProvider); //Execute the plugin
            return fakedPlugin;
        }

        public static IPlugin ExecutePluginWithConfigurations<T>(this IXrmFakedContext context, XrmFakedPluginExecutionContext plugCtx, string unsecureConfiguration, string secureConfiguration)
            where T : class, IPlugin
        {
            var pluginType = typeof(T);
            var constructors = pluginType.GetConstructors().ToList();

            if (!constructors.Any(c => c.GetParameters().Length == 2 && c.GetParameters().All(param => param.ParameterType == typeof(string))))
            {
                throw new ArgumentException("The plugin you are trying to execute does not specify a constructor for passing in two configuration strings.");
            }

            var pluginInstance = (T)Activator.CreateInstance(typeof(T), unsecureConfiguration, secureConfiguration);

            return context.ExecutePluginWith(plugCtx, pluginInstance);
        }

        [Obsolete("Use ExecutePluginWith(XrmFakedPluginExecutionContext ctx, IPlugin instance).")]
        public static IPlugin ExecutePluginWithConfigurations<T>(this IXrmFakedContext context, XrmFakedPluginExecutionContext plugCtx, T instance, string unsecureConfiguration="", string secureConfiguration="")
            where T : class, IPlugin
        {
            var fakedServiceProvider = context.PluginContextProperties.GetServiceProvider(plugCtx);

            var fakedPlugin = A.Fake<IPlugin>();

            A.CallTo(() => fakedPlugin.Execute(A<IServiceProvider>._))
                .Invokes((IServiceProvider provider) =>
                {
                    var pluginType = typeof(T);
                    var constructors = pluginType.GetConstructors();

                    if (!constructors.Any(c => c.GetParameters().Length == 2 && c.GetParameters().All(param => param.ParameterType == typeof(string))))
                    {
                        throw new ArgumentException("The plugin you are trying to execute does not specify a constructor for passing in two configuration strings.");
                    }

                    var plugin = instance;
                    plugin.Execute(fakedServiceProvider);
                });

            fakedPlugin.Execute(fakedServiceProvider); //Execute the plugin
            return fakedPlugin;
        }

        public static IPlugin ExecutePluginWithTarget<T>(this IXrmFakedContext context, XrmFakedPluginExecutionContext ctx, Entity target, string messageName = "Create", int stage = 40)
          where T : IPlugin, new()
        {
            ctx.InputParameters.Add("Target", target);
            ctx.MessageName = messageName;
            ctx.Stage = stage;

            return context.ExecutePluginWith<T>(ctx);
        }

        /// <summary>
        /// Executes the plugin of type T against the faked context for an entity target
        /// and returns the faked plugin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The entity to execute the plug-in for.</param>
        /// <param name="messageName">Sets the message name.</param>
        /// <param name="stage">Sets the stage.</param>
        /// <returns></returns>
        public static IPlugin ExecutePluginWithTarget<T>(this IXrmFakedContext context, Entity target, string messageName = "Create", int stage = 40)
            where T : IPlugin, new()
        {
            return context.ExecutePluginWithTarget(new T(), target, messageName, stage);
        }

        /// <summary>
        /// Executes the plugin of type T against the faked context for an entity target
        /// and returns the faked plugin
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="target">The entity to execute the plug-in for.</param>
        /// <param name="messageName">Sets the message name.</param>
        /// <param name="stage">Sets the stage.</param>
        /// <returns></returns>
        public static IPlugin ExecutePluginWithTarget(this IXrmFakedContext context, IPlugin instance, Entity target, string messageName = "Create", int stage = 40)
        {
            var ctx = context.GetDefaultPluginContext();

            // Add the target entity to the InputParameters
            ctx.InputParameters.Add("Target", target);
            ctx.MessageName = messageName;
            ctx.Stage = stage;

            return context.ExecutePluginWith(ctx, instance);
        }

        /// <summary>
        /// Executes the plugin of type T against the faked context for an entity reference target
        /// and returns the faked plugin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The entity reference to execute the plug-in for.</param>
        /// <param name="messageName">Sets the message name.</param>
        /// <param name="stage">Sets the stage.</param>
        /// <returns></returns>
        public static IPlugin ExecutePluginWithTargetReference<T>(this IXrmFakedContext context, EntityReference target, string messageName = "Delete", int stage = 40)
            where T : IPlugin, new()
        {
            return context.ExecutePluginWithTargetReference(new T(), target, messageName, stage);
        }

        /// <summary>
        /// Executes the plugin of type T against the faked context for an entity reference target
        /// and returns the faked plugin
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="target">The entity reference to execute the plug-in for.</param>
        /// <param name="messageName">Sets the message name.</param>
        /// <param name="stage">Sets the stage.</param>
        /// <returns></returns>
        public static IPlugin ExecutePluginWithTargetReference(this IXrmFakedContext context, IPlugin instance, EntityReference target, string messageName = "Delete", int stage = 40)
        {
            var ctx = context.GetDefaultPluginContext();
            // Add the target entity to the InputParameters
            ctx.InputParameters.Add("Target", target);
            ctx.MessageName = messageName;
            ctx.Stage = stage;

            return context.ExecutePluginWith(ctx, instance);
        }

        /// <summary>
        /// Returns a faked plugin with a target and the specified pre entity images
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Obsolete]
        public static IPlugin ExecutePluginWithTargetAndPreEntityImages<T>(this IXrmFakedContext context, object target, EntityImageCollection preEntityImages, string messageName = "Create", int stage = 40)
            where T : IPlugin, new()
        {
            var ctx = context.GetDefaultPluginContext();
            // Add the target entity to the InputParameters
            ctx.InputParameters.Add("Target", target);
            ctx.PreEntityImages.AddRange(preEntityImages);
            ctx.MessageName = messageName;
            ctx.Stage = stage;

            return context.ExecutePluginWith<T>(ctx);
        }

        /// <summary>
        /// Returns a faked plugin with a target and the specified post entity images
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Obsolete]
        public static IPlugin ExecutePluginWithTargetAndPostEntityImages<T>(this IXrmFakedContext context, object target, EntityImageCollection postEntityImages, string messageName = "Create", int stage = 40)
            where T : IPlugin, new()
        {
            var ctx = context.GetDefaultPluginContext();
            // Add the target entity to the InputParameters
            ctx.InputParameters.Add("Target", target);
            ctx.PostEntityImages.AddRange(postEntityImages);
            ctx.MessageName = messageName;
            ctx.Stage = stage;

            return context.ExecutePluginWith<T>(ctx);
        }

        [Obsolete]
        public static IPlugin ExecutePluginWithTargetAndInputParameters<T>(this IXrmFakedContext context, Entity target, ParameterCollection inputParameters, string messageName = "Create", int stage = 40)
            where T : IPlugin, new()
        {
            var ctx = context.GetDefaultPluginContext();

            ctx.InputParameters.AddRange(inputParameters);

            return context.ExecutePluginWithTarget<T>(ctx, target, messageName, stage);
        }

    }
}