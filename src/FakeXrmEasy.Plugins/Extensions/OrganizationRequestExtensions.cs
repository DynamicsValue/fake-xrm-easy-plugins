using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Plugins.Extensions
{
    public static class OrganizationRequestExtensions
    {
        public static bool IsBulkOperation(this OrganizationRequest request)
        {
            return IsCreateMultipleRequest(request) ||
                   IsUpdateMultipleRequest(request) ||
                   IsUpsertMultipleRequest(request);
        }
        
        public static bool IsCreateMultipleRequest(this OrganizationRequest request)
        {
            return "CreateMultiple".Equals(request.RequestName);
        }
        
        public static bool IsUpdateMultipleRequest(this OrganizationRequest request)
        {
            return "UpdateMultiple".Equals(request.RequestName);
        }
        
        public static bool IsUpsertMultipleRequest(this OrganizationRequest request)
        {
            return "UpsertMultiple".Equals(request.RequestName);
        }
        
        /*
        public static bool IsDeleteMultipleRequest(this OrganizationRequest request)
        {
            return "DeleteMultiple".Equals(request.RequestName);
        }
        */
    }
}