using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace FakeXrmEasy.Plugins.Extensions
{
    public static class OrganizationRequestExtensions
    {
        private const string CreateMultipleRequestName = "CreateMultiple";
        private const string UpdateMultipleRequestName = "UpdateMultiple";
        private const string UpsertMultipleRequestName = "UpsertMultiple";
        
        private const string CreateRequestName = "Create";
        private const string UpdateRequestName = "Update";
        private const string UpsertRequestName = "Upsert";
        
        public static bool IsBulkOperation(this OrganizationRequest request)
        {
            return IsCreateMultipleRequest(request) ||
                   IsUpdateMultipleRequest(request) ||
                   IsUpsertMultipleRequest(request);
        }
        
        public static bool IsCreateMultipleRequest(this OrganizationRequest request)
        {
            return CreateMultipleRequestName.Equals(request.RequestName);
        }
        
        public static bool IsUpdateMultipleRequest(this OrganizationRequest request)
        {
            return UpdateMultipleRequestName.Equals(request.RequestName);
        }
        
        public static bool IsUpsertMultipleRequest(this OrganizationRequest request)
        {
            return UpsertMultipleRequestName.Equals(request.RequestName);
        }
        
        /// <summary>
        /// Returns a request name for a non-bulk request from the current bulk request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetAssociatedNonBulkRequestName(this OrganizationRequest request)
        {
            if (CreateMultipleRequestName.Equals(request.RequestName))
                return CreateRequestName;
            
            if (UpdateMultipleRequestName.Equals(request.RequestName))
                return UpdateRequestName;
            
            if (UpsertMultipleRequestName.Equals(request.RequestName))
                return UpsertRequestName;

            throw new GetAssociatedNonBulkRequestNameException(request);
        }
        
        public static string GetAssociatedBulkRequestName(this OrganizationRequest request)
        {
            if (CreateRequestName.Equals(request.RequestName))
                return CreateMultipleRequestName;
            
            if (UpdateRequestName.Equals(request.RequestName))
                return UpdateMultipleRequestName;
            
            if (UpsertRequestName.Equals(request.RequestName))
                return UpsertMultipleRequestName;

            throw new GetAssociatedNonBulkRequestNameException(request);
        }
  
    }
}