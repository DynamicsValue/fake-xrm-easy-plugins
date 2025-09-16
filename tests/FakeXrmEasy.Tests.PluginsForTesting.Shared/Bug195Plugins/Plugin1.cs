using System;
using DataverseEntities;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Tests.PluginsForTesting.Bug195Plugins
{
    public class Plugin1: IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            var messageName = context.MessageName;
            tracing.Trace($"Message Name: {messageName}");

            var stage = context.Stage;
            tracing.Trace($"Stage: {stage}");

            var mode = context.Mode;
            tracing.Trace($"Mode: {mode}");

            object target;
            context.InputParameters.TryGetValue("Target", out target);

            var entity = target as Entity;
            if (entity != null)
            {
                tracing.Trace($"Entity Logical Name: {entity.LogicalName}");
                tracing.Trace($"Entity ID: {entity.Id}");
            }

            var entityReference = target as EntityReference;
            if (entityReference != null)
            {
                tracing.Trace($"Entity Reference Logical Name: {entityReference.LogicalName}");
                tracing.Trace($"Entity Reference ID: {entityReference.Id}");
            }

            var account = target as Account;
            
            //modify the target
            account.OwnerId = new EntityReference("systemuser", Guid.NewGuid());
            account.Telephone1 = "123";

            // var account = new Account()
            // {
            //     AccountId = entity.Id,
            //     OwnerId = new EntityReference("systemuser", Guid.NewGuid()),
            //     Telephone1 = "123"
            // };
            //
            // var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            // IOrganizationService service = serviceFactory.CreateOrganizationService(null);
            // service.Update(account);
        }
    }
}