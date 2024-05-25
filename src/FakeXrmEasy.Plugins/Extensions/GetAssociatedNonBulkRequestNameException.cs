using System;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Plugins.Extensions
{
    /// <summary>
    /// Exception thrown when a method was invoked on an organization request that is not a bulk operation
    /// </summary>
    public class GetAssociatedNonBulkRequestNameException: Exception
    {
        public GetAssociatedNonBulkRequestNameException(OrganizationRequest request): 
            base($"GetAssociatedNonBulkRequestName can not be called on a non-bulk organization request '{request.RequestName}'")
        {
            
        }
    }
}