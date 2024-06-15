using System;
using FakeItEasy;
using FakeXrmEasy.Abstractions.Plugins;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Plugins
{
    internal static class FakePluginFactory
    {
        /// <summary>
        /// Returns a fake plugin that will be executed using the provided fakeServiceProvider using a specific instance
        /// </summary>
        /// <param name="fakeServiceProvider"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        internal static IPlugin New(IServiceProvider fakeServiceProvider, IPlugin instance)
        {
            var fakedPlugin = A.Fake<IPlugin>();
            A.CallTo(() => fakedPlugin.Execute(A<IServiceProvider>._))
                .Invokes((IServiceProvider provider) =>
                {
                    var plugin = instance;
                    plugin.Execute(fakeServiceProvider);
                });
            
            return fakedPlugin;
        }
        
        /// <summary>
        /// Returns a fake plugin that will be executed using the provided fakeServiceProvider using the plugin parameterless constructor
        /// </summary>
        /// <param name="fakeServiceProvider"></param>
        /// <returns></returns>
        internal static IPlugin New<T>(IServiceProvider fakeServiceProvider) where T : IPlugin, new()
        {
            var fakedPlugin = A.Fake<IPlugin>();
            A.CallTo(() => fakedPlugin.Execute(A<IServiceProvider>._))
                .Invokes((IServiceProvider provider) =>
                {
                    var plugin = new T();
                    plugin.Execute(fakeServiceProvider);
                });
            
            return fakedPlugin;
        }
    }
}