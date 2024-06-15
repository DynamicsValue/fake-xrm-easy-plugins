using System;
using System.Collections.Generic;
using DataverseEntities;
using FakeXrmEasy.Pipeline;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline.RegisteredPluginStepsRetrieverTests
{
    public class GetOrganizationRequestEntityLogicalNameTests: FakeXrmEasyTestsBase
    {
        private readonly Account _account;
        
        private readonly UpdateRequest _updateRequest;
        private readonly DeleteRequest _deleteRequest;
        
        #if FAKE_XRM_EASY_9
        private readonly UpdateMultipleRequest _updateMultipleRequest;
        #endif
        
        
        public GetOrganizationRequestEntityLogicalNameTests()
        {
            _account = new Account() { Id = Guid.NewGuid() };
            _updateRequest = new UpdateRequest() { Target = _account };
            _deleteRequest = new DeleteRequest() { Target = _account.ToEntityReference() };
            
            #if FAKE_XRM_EASY_9
            _updateMultipleRequest = new UpdateMultipleRequest()
            {
                Targets = new EntityCollection(new List<Entity>() { _account })
                {
                    EntityName = Account.EntityLogicalName
                }
            };
            #endif
        }

        [Fact]
        public void Should_return_entity_logical_name_for_a_single_request()
        {
            var entityLogicalName = RegisteredPluginStepsRetriever.GetOrganizationRequestEntityLogicalName(_updateRequest);
            Assert.Equal(Account.EntityLogicalName, entityLogicalName);
        }
        
        [Fact]
        public void Should_return_entity_logical_name_for_a_single_entity_reference()
        {
            var entityLogicalName = RegisteredPluginStepsRetriever.GetOrganizationRequestEntityLogicalName(_deleteRequest);
            Assert.Equal(Account.EntityLogicalName, entityLogicalName);
        }
        
        #if FAKE_XRM_EASY_9
        [Fact]
        public void Should_return_entity_logical_name_for_a_multiple_request()
        {
            var entityLogicalName = RegisteredPluginStepsRetriever.GetOrganizationRequestEntityLogicalName(_updateMultipleRequest);
            Assert.Equal(Account.EntityLogicalName, entityLogicalName);
        }
        #endif
        
    }
}