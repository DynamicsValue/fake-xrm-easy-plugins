using System;

namespace FakeXrmEasy.Plugins.Extensions
{
    internal class InvalidBulkOperationExtensionException: Exception
    {
        internal InvalidBulkOperationExtensionException() : base("Bulk Operations Organization Request extensions are not supported on non-bulk OrganizationRequests")
        {
            
        }
    }
}