using System.Collections.Generic;
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
        
        public static bool IsNonBulkOperation(this OrganizationRequest request)
        {
            return IsCreateRequest(request) ||
                   IsUpdateRequest(request) ||
                   IsUpsertRequest(request);
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
        
        public static bool IsCreateRequest(this OrganizationRequest request)
        {
            return CreateRequestName.Equals(request.RequestName);
        }
        
        public static bool IsUpdateRequest(this OrganizationRequest request)
        {
            return UpdateRequestName.Equals(request.RequestName);
        }
        
        public static bool IsUpsertRequest(this OrganizationRequest request)
        {
            return UpsertRequestName.Equals(request.RequestName);
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

        /// <summary>
        /// Converts the current bulk operation organisation request into an array of multiple non-bulk organization requests
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static OrganizationRequest[] ToNonBulkOrganizationRequests(this OrganizationRequest request)
        {
            if (!IsBulkOperation(request))
            {
                throw new InvalidBulkOperationExtensionException();
            }

            var targets = request["Targets"] as EntityCollection;
            var entities = targets.Entities;
            
            var requestName = GetAssociatedNonBulkRequestName(request);

            var requests = new List<OrganizationRequest> { };
            foreach (var target in entities)
            {
                requests.Add(new OrganizationRequest()
                {
                    RequestName = requestName,
                    ["Target"] = target
                });
            }

            return requests.ToArray();
        }

        /// <summary>
        /// Converts a single non-bulk operation request into an equivalent bulk operation with a single record
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="InvalidBulkOperationExtensionException"></exception>
        public static OrganizationRequest ToBulkOrganizationRequest(this OrganizationRequest request)
        {
            if (IsBulkOperation(request))
            {
                throw new InvalidBulkOperationExtensionException();
            }

            var target = request["Target"] as Entity;

            var requestName = GetAssociatedBulkRequestName(request);

            return new OrganizationRequest()
            {
                RequestName = requestName,
                ["Targets"] = new EntityCollection(new List<Entity>()
                {
                    target
                })
                {
                  EntityName  = target.LogicalName
                }
            };
        }
    }
}