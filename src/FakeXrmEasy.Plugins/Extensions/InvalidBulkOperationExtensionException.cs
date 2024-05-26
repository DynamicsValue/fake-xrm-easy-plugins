using System;

namespace FakeXrmEasy.Plugins.Extensions
{
    public class InvalidBulkOperationExtensionException: Exception
    {
        public InvalidBulkOperationExtensionException() : base("Bulk Operations Organization Request extensions are not supported on non-bulk OrganizationRequests")
        {
            
        }
    }
}