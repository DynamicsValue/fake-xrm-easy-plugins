using System;

namespace FakeXrmEasy.Plugins.Extensions
{
    /// <summary>
    /// Exception raised when a bulk request is converted to a non bulk request but the original request wasn't a valid bulk request
    /// </summary>
    public class InvalidBulkOperationExtensionException: Exception
    {
        internal InvalidBulkOperationExtensionException() : base("Bulk Operations Organization Request extensions are not supported on non-bulk OrganizationRequests")
        {
            
        }
    }
}