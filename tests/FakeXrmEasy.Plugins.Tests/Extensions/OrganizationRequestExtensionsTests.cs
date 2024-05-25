using FakeXrmEasy.Plugins.Extensions;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Extensions
{
    public class OrganizationRequestExtensionsTests
    {
        private readonly OrganizationRequest _request;
        
        public OrganizationRequestExtensionsTests()
        {
            _request = new OrganizationRequest();
        }
        
        [Theory]
        [InlineData("CreateMultiple")]
        public void Should_return_true_for_create_multiple_when_asked_if_it_is_a_create_multiple_request(string requestName)
        {
            _request.RequestName = requestName;
            Assert.True(_request.IsCreateMultipleRequest());
        }
        
        [Theory]
        [InlineData("UpdateMultiple")]
        public void Should_return_true_for_update_multiple_when_asked_if_it_is_an_update_multiple_request(string requestName)
        {
            _request.RequestName = requestName;
            Assert.True(_request.IsUpdateMultipleRequest());
        }
        
        [Theory]
        [InlineData("UpsertMultiple")]
        public void Should_return_true_for_upsert_multiple_when_asked_if_it_is_an_upsert_multiple_request(string requestName)
        {
            _request.RequestName = requestName;
            Assert.True(_request.IsUpsertMultipleRequest());
        }
        
        [Theory]
        [InlineData("CreateMultiple")]
        [InlineData("UpdateMultiple")]
        [InlineData("UpsertMultiple")]
        public void Should_return_true_when_its_a_bulk_operation(string requestName)
        {
            _request.RequestName = requestName;
            Assert.True(_request.IsBulkOperation());
        }
        
        [Theory]
        [InlineData("Create")]
        [InlineData("Assign")]
        public void Should_return_false_when_it_is_not_a_bulk_operation(string requestName)
        {
            _request.RequestName = requestName;
            Assert.False(_request.IsBulkOperation());
        }
        
        [Theory]
        [InlineData("CreateMultiple", "Create")]
        [InlineData("UpdateMultiple", "Update")]
        [InlineData("UpsertMultiple", "Upsert")]
        public void Should_return_associated_non_bulk_request_name_for_bulk_operation(string bulkOperation, string nonBulkOperation)
        {
            _request.RequestName = bulkOperation;
            var nonBulkRequestName = _request.GetAssociatedNonBulkRequestName();
            Assert.Equal(nonBulkOperation, nonBulkRequestName);
        }
        
        [Theory]
        [InlineData("Create", "CreateMultiple")]
        [InlineData("Update", "UpdateMultiple")]
        [InlineData("Upsert", "UpsertMultiple")]
        public void Should_return_associated_bulk_request_name_for_non_bulk_operation(string nonBulkOperation, string bulkOperation)
        {
            _request.RequestName = nonBulkOperation;
            var nonBulkRequestName = _request.GetAssociatedBulkRequestName();
            Assert.Equal(bulkOperation, nonBulkRequestName);
        }
    }
}