using System.Collections.Generic;
using DataverseEntities;
using FakeXrmEasy.Plugins.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Extensions
{
    public class OrganizationRequestExtensionsNonBulkOperationsConversionTests
    {
        private CreateRequest _createRequest;
        private UpdateRequest _updateRequest;
        
        #if !FAKE_XRM_EASY_2013 && !FAKE_XRM_EASY
        private UpsertRequest _upsertRequest;
        #endif
        
        private readonly Account _account1;
        private readonly Account _account2;

        private readonly EntityCollection _entityCollection;
        
        public OrganizationRequestExtensionsNonBulkOperationsConversionTests()
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
        public void Should_convert_create_operation_into_a_create_multiple_request_with_a_single_record()
        {
            _createRequest = new CreateRequest()
            {
                Target = _account1
            };

            var request = _createRequest.ToBulkOrganizationRequest();
            Assert.NotNull(request);

            AssertIsBulkOperationWithSingleRecord(request, "CreateMultiple", _account1);
        }
        
        
        [Fact]
        public void Should_convert_update_operation_into_a_update_multiple_request_with_a_single_record()
        {
            _updateRequest = new UpdateRequest()
            {
                Target = _account1
            };

            var request = _updateRequest.ToBulkOrganizationRequest();
            Assert.NotNull(request);

            AssertIsBulkOperationWithSingleRecord(request, "UpdateMultiple", _account1);
        }
        
#if !FAKE_XRM_EASY_2013 && !FAKE_XRM_EASY
        [Fact]
        public void Should_convert_upsert_operation_into_a_upsert_multiple_request_with_a_single_record()
        {
            _upsertRequest = new UpsertRequest()
            {
                Target = _account1
            };

            var request = _upsertRequest.ToBulkOrganizationRequest();
            Assert.NotNull(request);

            AssertIsBulkOperationWithSingleRecord(request, "UpsertMultiple", _account1);
        }
#endif
        
        private void AssertIsBulkOperationWithSingleRecord(OrganizationRequest request, string requestName, Entity record)
        {
            var entityCollection = request["Targets"] as EntityCollection;
            Assert.Equal(requestName, request.RequestName);
            Assert.Equal(record, entityCollection.Entities[0]);
        }
    }
}