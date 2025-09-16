using System;
using DataverseEntities;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Tests.PluginsForTesting.Bug195Plugins;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline
{
    public class Bug195Tests: FakeXrmEasyPipelineTestsBase
    {
        private readonly Account _account;
        private readonly Contact _contact;

        public Bug195Tests()
        {
            _account = new Account()
            {
                Id = Guid.NewGuid(),
                AccountNumber = "1234567890",
                AccountCategoryCode = account_accountcategorycode.PreferredCustomer,
                NumberOfEmployees = 5,
                Revenue = new Money(20000),
                Telephone1 = "+123456"
            };

            _contact = new Contact()
            {
                Id = Guid.NewGuid()
            };
        }
        
        [Fact]
        public void Should_trigger_other_preoperation_plugins_with_higher_rank()
        {
            _context.Initialize(_account);
            _context.RegisterPluginStep<Plugin1>(new PluginStepDefinition()
            {
                MessageName = "Update",
                EntityLogicalName = Account.EntityLogicalName,
                Stage = ProcessingStepStage.Preoperation,
                Mode = ProcessingStepMode.Synchronous,
                FilteringAttributes = new string[] { "name" },
                Rank = 1
            });

            _context.RegisterPluginStep<Plugin2>(new PluginStepDefinition()
            {
                MessageName = "Update",
                EntityLogicalName = Account.EntityLogicalName,
                Stage = ProcessingStepStage.Preoperation,
                Mode = ProcessingStepMode.Synchronous,
                FilteringAttributes = new string[] { "ownerid", "telephone1" },
                Rank = 2
            });

            // Act
            var target = new Account
            {
                Id = _account.Id,
                Name = "name"
            };

            _service.Update(target);

            // Assert
            var trace = _context.GetTracingService().DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(10, trace.Length);
            Assert.Contains("Message Name: Update", trace);
            Assert.Contains("Stage: 20", trace);
            Assert.Contains("Mode: 0", trace);
            Assert.Contains($"Entity Logical Name: {Account.EntityLogicalName}", trace);
            Assert.Contains($"Entity ID: {_account.Id}", trace);
        }
    }
}