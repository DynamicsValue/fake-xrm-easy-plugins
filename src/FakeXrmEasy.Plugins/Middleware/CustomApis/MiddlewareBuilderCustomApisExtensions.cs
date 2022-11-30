using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Abstractions.Middleware;
using FakeXrmEasy.Middleware.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.Reflection;

namespace FakeXrmEasy.Plugins.Middleware.CustomApis
{
    /// <summary>
    /// Extension methods to configure custom api executions in middleware
    /// </summary>
    public static class MiddlewareBuilderCustomApisExtensions
    {
        /// <summary>
        /// Adds all custom api fake message executors that exist in the selected assembly
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IMiddlewareBuilder AddCustomApiFakeMessageExecutors(this IMiddlewareBuilder builder, Assembly assembly = null)
        {
            builder.Add(context => {
                if (assembly == null) assembly = Assembly.GetExecutingAssembly();
                var service = context.GetOrganizationService();

                var fakeCustomApiExecutorsDictionary =
                    assembly
                    .GetTypes()
                    .Where(t => t.GetInterfaces().Contains(typeof(ICustomApiFakeMessageExecutor)))
                    .Select(t => Activator.CreateInstance(t) as ICustomApiFakeMessageExecutor)
                    .ToDictionary(t => t.GetResponsibleRequestType(), t => (IFakeMessageExecutor) t);

                var messageExecutors = new MessageExecutors(fakeCustomApiExecutorsDictionary);

                if (!context.HasProperty<MessageExecutors>())
                    context.SetProperty(messageExecutors);
                else
                {
                    foreach (var messageExecutorKey in messageExecutors.Keys)
                    {
                        builder.AddFakeMessageExecutor(messageExecutors[messageExecutorKey]);
                    }
                }
            });

            return builder;
        }
    }
}
