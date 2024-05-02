using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace FakeXrmEasy.Plugins.Tests.Pipeline.BulkOperations
{
    public static class BulkOperationsExtensions
    {
        public static CreateMultipleRequest ToCreateMultipleRequest(this List<Entity> entities)
        {
            var entityCollection = new EntityCollection(entities)
            {
                EntityName = entities.First().LogicalName
            };

            return new CreateMultipleRequest()
            {
                Targets = entityCollection
            };
        }
    }
}