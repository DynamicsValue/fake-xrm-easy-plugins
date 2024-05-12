using System;
using System.Collections.Generic;
using DataverseEntities;
using FakeXrmEasy.Pipeline;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline.RegisteredPluginStepsRetrieverTests
{
    /// <summary>
    /// Tests that calculate the distinct attributes that were sent in a given organization request for purposes of plugin step attribute filtering in pipeline simulation
    /// </summary>
    public class GetOrganizationRequestFilteringAttributesTests: FakeXrmEasyTestsBase
    {
        private readonly Account _account;
        private readonly UpdateRequest _updateRequest;

        public GetOrganizationRequestFilteringAttributesTests()
        {
            _account = new Account() { Id = Guid.NewGuid() };
            _updateRequest = new UpdateRequest() { Target = _account };
        }
        
        [Fact]
        public void Should_return_primary_key_when_no_explicit_attributes_are_added()
        {
            var attributes = RegisteredPluginStepsRetriever.GetOrganizationRequestFilteringAttributes(_updateRequest);
            Assert.NotEmpty(attributes);

            Assert.Contains("accountid", attributes);
        }
        
        [Fact]
        public void Should_return_attributes_in_an_update_message()
        {
            _account.Telephone1 = "tel_1";
            _account.Name = "Test account";
            
            var attributes = RegisteredPluginStepsRetriever.GetOrganizationRequestFilteringAttributes(_updateRequest);
            Assert.NotEmpty(attributes);
            
            Assert.Contains("telephone1", attributes);
            Assert.Contains("name", attributes);
        }
        
        #if FAKE_XRM_EASY_9
        [Fact]
        public void Should_return_any_different_attributes_in_an_any_of_the_entities_of_an_update_multiple_message()
        {
            /* With UpdateMultiple, the plug-in runs when any of the selected attributes are included in any of the entities in the Targets parameter. */
            //https://learn.microsoft.com/en-us/power-apps/developer/data-platform/write-plugin-multiple-operation?tabs=single#attribute-filters
            
            var account1 = new Account() { Id = Guid.NewGuid(), Name = "test account" };
            var account2 = new Account() { Id = Guid.NewGuid(), Telephone1 = "tel 1" };
            var account3 = new Account() { Id = Guid.NewGuid(), Telephone1 = "tel 2" };
            
            var updateMultipleRequest = new UpdateMultipleRequest()
            {
                Targets = new EntityCollection(new List<Entity>() { account1, account2, account3 })
                {
                    EntityName = Account.EntityLogicalName
                }
            };
            
            var attributes = RegisteredPluginStepsRetriever.GetOrganizationRequestFilteringAttributes(updateMultipleRequest);
            Assert.NotEmpty(attributes);
            
            Assert.Contains("telephone1", attributes);
            Assert.Contains("name", attributes);
        }
        #endif
    }
}