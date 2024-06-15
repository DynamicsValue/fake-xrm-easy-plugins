using System;
using Crm;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Tests.PluginsForTesting
{
    /// <summary>
    /// Test plugin used to test plugin depth
    /// </summary>
    public class DepthPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var entity = (Entity)context.InputParameters["Target"];

            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(null);

            if (context.Depth > 1) return;
            
            var updatedEntity = new Account { Id = entity.Id, Name = "Updated" };
            service.Update(updatedEntity);
        }
    }
}
