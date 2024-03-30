using System;
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk;
using FakeItEasy;
using FakeXrmEasy.Abstractions.Plugins;
using System.Linq;

namespace FakeXrmEasy.Plugins
{
    /// <summary>
    /// Adds extension methods to an IXrmBaseContext useful for developing and testing plugins
    /// </summary>
    public static class IXrmBaseContextPluginExtensions
    {
        /// <summary>
        /// Returns a plugin context with default properties one can override
        /// </summary>
        /// <returns></returns>
        public static XrmFakedPluginExecutionContext GetDefaultPluginContext(this IXrmBaseContext context)
        {
            var userId = context.CallerProperties.CallerId?.Id ?? Guid.NewGuid();
            Guid businessUnitId = context.CallerProperties.BusinessUnitId?.Id ?? Guid.NewGuid();

            var plugCtx = XrmFakedPluginExecutionContext.New();

            plugCtx.Depth = 1;
            plugCtx.IsExecutingOffline = false;
            plugCtx.MessageName = "Create";
            plugCtx.UserId = userId;
            plugCtx.InitiatingUserId = userId;
            plugCtx.BusinessUnitId = businessUnitId;
            plugCtx.InputParameters = new ParameterCollection();
            plugCtx.OutputParameters = new ParameterCollection();
            plugCtx.SharedVariables = new ParameterCollection();
            plugCtx.PreEntityImages = new EntityImageCollection();
            plugCtx.PostEntityImages = new EntityImageCollection();
            plugCtx.IsolationMode = 1;

            return plugCtx;
        }

        /// <summary>
        /// Returns the default plugin context properties of this IXrmBaseContext
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IXrmFakedPluginContextProperties GetPluginContextProperties(this IXrmBaseContext context) 
        {
            if(context.PluginContextProperties == null) 
            {
                context.PluginContextProperties = new XrmFakedPluginContextProperties(context, context.GetOrganizationService(), context.GetTracingService());
            }
            return context.PluginContextProperties;
        }

        /// <summary>
        /// Executes a plugin passing a custom context. This is useful whenever we need to mock more complex plugin contexts (ex: passing MessageName, plugin Depth, InitiatingUserId etc...)
        /// </summary>
        /// <param name="context">The IXrmBaseContext instance where this plugin will be executed</param>
        /// <param name="plugCtx">A plugin context with the minimum set of properties needed for the plugin execution</param>
        /// <param name="instance">A specific plugin instance, where you might have already setup / injected other dependencies</param>
        /// <returns></returns>
        public static IPlugin ExecutePluginWith(this IXrmBaseContext context, XrmFakedPluginExecutionContext plugCtx, IPlugin instance)
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
        /// <param name="context">The IXrmBaseContext instance where this plugin will be executed</param>
        /// <param name="ctx">A plugin context with the minimum set of properties needed for the plugin execution</param>
        /// <returns></returns>
        public static IPlugin ExecutePluginWith<T>(this IXrmBaseContext context, XrmFakedPluginExecutionContext ctx = null)
            where T : IPlugin, new()
        {
            if (ctx == null)
            {
                ctx = context.GetDefaultPluginContext();
            }

            return context.ExecutePluginWith(ctx, new T());
        }

        /// <summary>
        /// Method to execute a plugin that takes several plugin execution properties as parameters. Soon to be deprecated, use the ExecutePluginWith&lt;T&gt; that takes a plugin context instance instead
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="inputParameters"></param>
        /// <param name="outputParameters"></param>
        /// <param name="preEntityImages"></param>
        /// <param name="postEntityImages"></param>
        /// <returns></returns>
        public static IPlugin ExecutePluginWith<T>(this IXrmBaseContext context, ParameterCollection inputParameters, ParameterCollection outputParameters, EntityImageCollection preEntityImages, EntityImageCollection postEntityImages)
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


        /// <summary>
        /// Executes a plugin with the unsecure and secure configurations specified
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="plugCtx"></param>
        /// <param name="unsecureConfiguration"></param>
        /// <param name="secureConfiguration"></param>
        /// <returns></returns>
        public static IPlugin ExecutePluginWithConfigurations<T>(this IXrmBaseContext context, XrmFakedPluginExecutionContext plugCtx, string unsecureConfiguration, string secureConfiguration)
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

        /// <summary>
        /// Method to execute a plugin with configurations that also takes a specific plugin instance where you might already injected other external dependencies
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="plugCtx"></param>
        /// <param name="instance"></param>
        /// <param name="unsecureConfiguration"></param>
        /// <param name="secureConfiguration"></param>
        /// <returns></returns>
        [Obsolete("Use ExecutePluginWith(XrmFakedPluginExecutionContext ctx, IPlugin instance).")]
        public static IPlugin ExecutePluginWithConfigurations<T>(this IXrmBaseContext context, XrmFakedPluginExecutionContext plugCtx, T instance, string unsecureConfiguration="", string secureConfiguration="")
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

        /// <summary>
        /// Executes a plugin with a given target
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="ctx"></param>
        /// <param name="target"></param>
        /// <param name="messageName"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        public static IPlugin ExecutePluginWithTarget<T>(this IXrmBaseContext context, XrmFakedPluginExecutionContext ctx, Entity target, string messageName = "Create", int stage = 40)
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
        /// <param name="context"></param>
        /// <param name="target">The entity to execute the plug-in for.</param>
        /// <param name="messageName">Sets the message name.</param>
        /// <param name="stage">Sets the stage.</param>
        /// <returns></returns>
        public static IPlugin ExecutePluginWithTarget<T>(this IXrmBaseContext context, Entity target, string messageName = "Create", int stage = 40)
            where T : IPlugin, new()
        {
            return context.ExecutePluginWithTarget(new T(), target, messageName, stage);
        }

        /// <summary>
        /// Executes the plugin of type T against the faked context for an entity target
        /// and returns the faked plugin
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instance"></param>
        /// <param name="target">The entity to execute the plug-in for.</param>
        /// <param name="messageName">Sets the message name.</param>
        /// <param name="stage">Sets the stage.</param>
        /// <returns></returns>
        public static IPlugin ExecutePluginWithTarget(this IXrmBaseContext context, IPlugin instance, Entity target, string messageName = "Create", int stage = 40)
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
        /// <param name="context"></param>
        /// <param name="target">The entity reference to execute the plug-in for.</param>
        /// <param name="messageName">Sets the message name.</param>
        /// <param name="stage">Sets the stage.</param>
        /// <returns></returns>
        public static IPlugin ExecutePluginWithTargetReference<T>(this IXrmBaseContext context, EntityReference target, string messageName = "Delete", int stage = 40)
            where T : IPlugin, new()
        {
            return context.ExecutePluginWithTargetReference(new T(), target, messageName, stage);
        }

        /// <summary>
        /// Executes the plugin of type T against the faked context for an entity reference target
        /// and returns the faked plugin
        /// </summary>
        /// <param name="context">The IXrmBaseContext used to execute the plugin</param>
        /// <param name="instance">A specific plugin instance</param>
        /// <param name="target">The entity reference to execute the plug-in for.</param>
        /// <param name="messageName">Sets the message name.</param>
        /// <param name="stage">Sets the stage.</param>
        /// <returns></returns>
        public static IPlugin ExecutePluginWithTargetReference(this IXrmBaseContext context, IPlugin instance, EntityReference target, string messageName = "Delete", int stage = 40)
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
        [Obsolete("Use ExecutePluginWith<T> instead")]
        public static IPlugin ExecutePluginWithTargetAndPreEntityImages<T>(this IXrmBaseContext context, object target, EntityImageCollection preEntityImages, string messageName = "Create", int stage = 40)
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
        /// <param name="context"></param>
        /// <param name="target"></param>
        /// <param name="postEntityImages"></param>
        /// <param name="messageName"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        [Obsolete("Use ExecutePluginWith<T> instead")]
        public static IPlugin ExecutePluginWithTargetAndPostEntityImages<T>(this IXrmBaseContext context, object target, EntityImageCollection postEntityImages, string messageName = "Create", int stage = 40)
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="target"></param>
        /// <param name="inputParameters"></param>
        /// <param name="messageName"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        [Obsolete("Use ExecutePluginWith<T> instead")]
        public static IPlugin ExecutePluginWithTargetAndInputParameters<T>(this IXrmBaseContext context, Entity target, ParameterCollection inputParameters, string messageName = "Create", int stage = 40)
            where T : IPlugin, new()
        {
            var ctx = context.GetDefaultPluginContext();

            ctx.InputParameters.AddRange(inputParameters);

            return context.ExecutePluginWithTarget<T>(ctx, target, messageName, stage);
        }
    }
}
