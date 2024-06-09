#if FAKE_XRM_EASY_9
using System.Collections.Generic;
using DataverseEntities;
using FakeXrmEasy.Plugins.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Extensions
{
    public class OrganizationRequestExtensionsBulkOperationsConversionTests
    {
        private CreateMultipleRequest _createMultipleRequest;
        private UpdateMultipleRequest _updateMultipleRequest;
        private UpsertMultipleRequest _upsertMultipleRequest;
        
        private readonly Account _account1;
        private readonly Account _account2;

        private readonly EntityCollection _entityCollection;
        
        public OrganizationRequestExtensionsBulkOperationsConversionTests()
        {
            _account1 = new Account() { };
            _account2 = new Account() { };

            var entities = new List<Entity>()
            {
                _account1, _account2
            };

            _entityCollection = new EntityCollection(entities)
            {
                EntityName = Account.EntityLogicalName
            };
        }
        
        [Fact]
        public void Should_convert_create_multiple_operation_into_an_array_of_create_requests()
        {
            _createMultipleRequest = new CreateMultipleRequest()
            {
                Targets = _entityCollection
            };

            var requests = _createMultipleRequest.ToNonBulkOrganizationRequests();
            Assert.NotNull(requests);
            
            Assert.Equal(2, requests.Length);
            
            AssertIsNonBulkOperation(requests[0], "Create", _account1);
            AssertIsNonBulkOperation(requests[1], "Create", _account2);
        }
        
        [Fact]
        public void Should_convert_update_multiple_operation_into_an_array_of_update_requests()
        {
            _updateMultipleRequest = new UpdateMultipleRequest()
            {
                Targets = _entityCollection
            };

            var requests = _updateMultipleRequest.ToNonBulkOrganizationRequests();
            Assert.NotNull(requests);
            
            Assert.Equal(2, requests.Length);
            
            AssertIsNonBulkOperation(requests[0], "Update", _account1);
            AssertIsNonBulkOperation(requests[1], "Update", _account2);
        }
        
        [Fact]
        public void Should_convert_upsert_multiple_operation_into_an_array_of_upsert_requests()
        {
            _upsertMultipleRequest = new UpsertMultipleRequest()
            {
                Targets = _entityCollection
            };

            var requests = _upsertMultipleRequest.ToNonBulkOrganizationRequests();
            Assert.NotNull(requests);
            
            Assert.Equal(2, requests.Length);
            
            AssertIsNonBulkOperation(requests[0], "Upsert", _account1);
            AssertIsNonBulkOperation(requests[1], "Upsert", _account2);
        }

        private void AssertIsNonBulkOperation(OrganizationRequest request, string requestName, Entity record)
        {
            Assert.Equal(requestName, request.RequestName);
            Assert.Equal(record, request["Target"]);
        }
    }
}
#endif