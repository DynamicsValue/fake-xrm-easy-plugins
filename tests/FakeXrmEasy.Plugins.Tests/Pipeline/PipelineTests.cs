using System;
using System.Collections.Generic;
using System.Linq;
using Crm;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;
using FakeXrmEasy.Abstractions.Plugins.Enums;

using FakeXrmEasy.Pipeline;

namespace FakeXrmEasy.Plugins.Tests.Pipeline
{
    public class PipelineTests: FakeXrmEasyPipelineTests
    {
        private readonly Account _account;

        public PipelineTests()
        {
            _account = new Account()
            {
                Id = Guid.NewGuid(),
                AccountNumber = "1234567890",
                AccountCategoryCode = new OptionSetValue(1),
                NumberOfEmployees = 5,
                Revenue = new Money(20000),
                Telephone1 = "+123456"
            };
        }

        [Fact]
        public void When_AccountNumberPluginIsRegisteredAsPluginStep_And_OtherPluginCreatesAnAccount_Expect_AccountNumberIsSet_InPreOperation()
        {
            _context.RegisterPluginStep<AccountNumberPlugin>("Create", ProcessingStepStage.Preoperation);

            _context.ExecutePluginWith<CreateAccountPlugin>();

            var account = _context.CreateQuery<Account>().FirstOrDefault();
            Assert.NotNull(account);
            Assert.True(account.Attributes.ContainsKey("accountnumber"));
            Assert.NotNull(account["accountnumber"]);
        }

        [Fact]
        public void When_PluginIsRegisteredWithEntity_And_OtherPluginCreatesAnAccount_Expect_AccountNumberIsNotSet_In_PostOperation()
        {
            _context.RegisterPluginStep<AccountNumberPlugin, Account>("Create");

            _context.ExecutePluginWith<CreateAccountPlugin>();

            var account = _context.CreateQuery<Account>().FirstOrDefault();
            Assert.NotNull(account);
            Assert.False(account.Attributes.ContainsKey("accountnumber"));
        }

        [Fact]
        public void When_PluginIsRegisteredForOtherEntity_And_OtherPluginCreatesAnAccount_Expect_AccountNumberIsNotSet()
        {
            _context.RegisterPluginStep<AccountNumberPlugin, Contact>("Create");

            _context.ExecutePluginWith<CreateAccountPlugin>();

            var account = _context.CreateQuery<Account>().FirstOrDefault();
            Assert.NotNull(account);
            Assert.False(account.Attributes.ContainsKey("accountnumber"));
        }

        [Fact]
        public void When_PluginStepRegisteredAsDeletePreOperationSyncronous_Expect_CorrectValues()
        {
            // Arange
            var id = Guid.NewGuid();

            var entities = new List<Entity>
            {
                new Contact
                {
                    Id = id
                }
            };
            _context.Initialize(entities);

            // Act
            _context.RegisterPluginStep<ValidatePipelinePlugin, Contact>("Delete", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous);

            var service = _context.GetOrganizationService();
            service.Delete(Contact.EntityLogicalName, id);

            // Assert
            var tracingService = _context.GetTracingService();
            var trace = tracingService.DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Delete", trace);
            Assert.Contains("Stage: 20", trace);
            Assert.Contains("Mode: 0", trace);
            Assert.Contains($"Entity Reference Logical Name: {Contact.EntityLogicalName}", trace);
            Assert.Contains($"Entity Reference ID: {id}", trace);
        }

        [Fact]
        public void When_PluginStepRegisteredAsDeletePostOperationSyncronous_Expect_CorrectValues()
        {
            // Arange
            var id = Guid.NewGuid();

            var entities = new List<Entity>
            {
                new Contact
                {
                    Id = id
                }
            };
            _context.Initialize(entities);

            // Act
            _context.RegisterPluginStep<ValidatePipelinePlugin, Contact>("Delete", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous);

            var service = _context.GetOrganizationService();
            service.Delete(Contact.EntityLogicalName, id);

            // Assert
            var tracingService = _context.GetTracingService();
            var trace = tracingService.DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Delete", trace);
            Assert.Contains("Stage: 40", trace);
            Assert.Contains("Mode: 0", trace);
            Assert.Contains($"Entity Reference Logical Name: {Contact.EntityLogicalName}", trace);
            Assert.Contains($"Entity Reference ID: {id}", trace);
        }

        [Fact]
        public void When_PluginStepRegisteredAsDeletePostOperationAsyncronous_Expect_CorrectValues()
        {
            // Arange
            var id = Guid.NewGuid();

            var entities = new List<Entity>
            {
                new Contact
                {
                    Id = id
                }
            };
            _context.Initialize(entities);

            // Act
            _context.RegisterPluginStep<ValidatePipelinePlugin, Contact>("Delete", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous);

            var service = _context.GetOrganizationService();
            service.Delete(Contact.EntityLogicalName, id);

            // Assert
            var tracingService = _context.GetTracingService();
            var trace = tracingService.DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Delete", trace);
            Assert.Contains("Stage: 40", trace);
            Assert.Contains("Mode: 1", trace);
            Assert.Contains($"Entity Reference Logical Name: {Contact.EntityLogicalName}", trace);
            Assert.Contains($"Entity Reference ID: {id}", trace);
        }

        [Fact]
        public void When_PluginStepRegisteredAsUpdatePreOperationSyncronous_Expect_CorrectValues()
        {
            // Arange
            var context = _context as XrmFakedContext;

            var id = Guid.NewGuid();

            var entities = new List<Entity>
            {
                new Contact
                {
                    Id = id
                }
            };
            context.Initialize(entities);

            // Act
            context.RegisterPluginStep<ValidatePipelinePlugin, Contact>("Update", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous);

            var updatedEntity = new Contact
            {
                Id = id
            };

            var service = context.GetOrganizationService();
            service.Update(updatedEntity);

            // Assert
            var trace = _context.GetTracingService().DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Update", trace);
            Assert.Contains("Stage: 20", trace);
            Assert.Contains("Mode: 0", trace);
            Assert.Contains($"Entity Logical Name: {Contact.EntityLogicalName}", trace);
            Assert.Contains($"Entity ID: {id}", trace);
        }

        [Fact]
        public void When_PluginStepRegisteredAsUpdatePostOperationSyncronous_Expect_CorrectValues()
        {
            // Arange
            var context = _context as XrmFakedContext;

            var id = Guid.NewGuid();

            var entities = new List<Entity>
            {
                new Contact
                {
                    Id = id
                }
            };
            context.Initialize(entities);

            // Act
            context.RegisterPluginStep<ValidatePipelinePlugin, Contact>("Update", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous);

            var updatedEntity = new Contact
            {
                Id = id
            };

            var service = context.GetOrganizationService();
            service.Update(updatedEntity);

            // Assert
            var trace = _context.GetTracingService().DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Update", trace);
            Assert.Contains("Stage: 40", trace);
            Assert.Contains("Mode: 0", trace);
            Assert.Contains($"Entity Logical Name: {Contact.EntityLogicalName}", trace);
            Assert.Contains($"Entity ID: {id}", trace);
        }

        [Fact]
        public void When_PluginStepRegisteredAsUpdatePostOperationAsyncronous_Expect_CorrectValues()
        {
            // Arange
            var context = _context as XrmFakedContext;

            var id = Guid.NewGuid();

            var entities = new List<Entity>
            {
                new Contact
                {
                    Id = id
                }
            };
            context.Initialize(entities);

            // Act
            context.RegisterPluginStep<ValidatePipelinePlugin, Contact>("Update", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous);

            var updatedEntity = new Contact
            {
                Id = id
            };

            var service = context.GetOrganizationService();
            service.Update(updatedEntity);

            // Assert
            var trace = _context.GetTracingService().DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Update", trace);
            Assert.Contains("Stage: 40", trace);
            Assert.Contains("Mode: 1", trace);
            Assert.Contains($"Entity Logical Name: {Contact.EntityLogicalName}", trace);
            Assert.Contains($"Entity ID: {id}", trace);
        }
        
        [Fact]
        public void When_PluginStepRegisteredAsCreatePreOperationSyncronous_Expect_CorrectValues()
        {
            // Arange
            var context = _context as XrmFakedContext;

            var id = Guid.NewGuid();

            // Act
            context.RegisterPluginStep<ValidatePipelinePlugin, Contact>("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous);

            var newEntity = new Contact
            {
                Id = id
            };

            var service = context.GetOrganizationService();
            service.Create(newEntity);

            // Assert
            var trace = _context.GetTracingService().DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Create", trace);
            Assert.Contains("Stage: 20", trace);
            Assert.Contains("Mode: 0", trace);
            Assert.Contains($"Entity Logical Name: {Contact.EntityLogicalName}", trace);
            Assert.Contains($"Entity ID: {id}", trace);
        }

        [Fact]
        public void When_PluginStepRegisteredAsCreatePostOperationSyncronous_Expect_CorrectValues()
        {
            // Arange
            var context = _context as XrmFakedContext;

            var id = Guid.NewGuid();

            // Act
            context.RegisterPluginStep<ValidatePipelinePlugin, Contact>("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous);

            var newEntity = new Contact
            {
                Id = id
            };

            var service = context.GetOrganizationService();
            service.Create(newEntity);

            // Assert
            var trace = _context.GetTracingService().DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Create", trace);
            Assert.Contains("Stage: 40", trace);
            Assert.Contains("Mode: 0", trace);
            Assert.Contains($"Entity Logical Name: {Contact.EntityLogicalName}", trace);
            Assert.Contains($"Entity ID: {id}", trace);
        }

        [Fact]
        public void When_PluginStepRegisteredAsCreatePostOperationAsyncronous_Expect_CorrectValues()
        {
            // Arange
            var context = _context as XrmFakedContext;

            var id = Guid.NewGuid();

            // Act
            context.RegisterPluginStep<ValidatePipelinePlugin, Contact>("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous);

            var newEntity = new Contact
            {
                Id = id
            };

            var service = context.GetOrganizationService();
            service.Create(newEntity);

            // Assert
            var trace = _context.GetTracingService().DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Create", trace);
            Assert.Contains("Stage: 40", trace);
            Assert.Contains("Mode: 1", trace);
            Assert.Contains($"Entity Logical Name: {Contact.EntityLogicalName}", trace);
            Assert.Contains($"Entity ID: {id}", trace);
        }

        [Fact]
        public void When_PluginStepRegisteredAsCreatePostOperation_Entity_Available()
        {
            var context = _context as XrmFakedContext;

            var target = new Account
            {
                Id = Guid.NewGuid(),
                Name = "Original"
            };

            context.RegisterPluginStep<PostOperationUpdatePlugin>("Create");
            IOrganizationService service = context.GetOrganizationService();

            service.Create(target);

            var updatedAccount = service.Retrieve(Account.EntityLogicalName, target.Id, new ColumnSet(true)).ToEntity<Account>();

            Assert.Equal("Updated", updatedAccount.Name);
        }

        [Theory]
        [InlineData("accountnumber")]
        [InlineData("AccountNumber")]
        public void Should_trigger_plugin_for_filtered_attribute_if_a_registration_exists(string attributeName)
        {
            _context.Initialize(_account);
            _context.RegisterPluginStep<ValidatePipelinePlugin, Account>("Update", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous, filteringAttributes: new string[] { attributeName });

            // Act
            var target = new Account
            {
                Id = _account.Id,
                AccountNumber = "01234"
            };

            _service.Update(target);

            // Assert
            var trace = _context.GetTracingService().DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Update", trace);
            Assert.Contains("Stage: 20", trace);
            Assert.Contains("Mode: 0", trace);
            Assert.Contains($"Entity Logical Name: {Account.EntityLogicalName}", trace);
            Assert.Contains($"Entity ID: {_account.Id}", trace);
        }

        [Theory]
        [InlineData("accountnumber")]
        [InlineData("AccountNumber")]
        public void Should_trigger_plugin_for_filtered_attribute_if_a_registration_exists_with_multiple_attributes(string attributeName)
        {
            _context.Initialize(_account);
            _context.RegisterPluginStep<ValidatePipelinePlugin, Account>("Update", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous, filteringAttributes: new string[] { attributeName, "name" });

            // Act
            var target = new Account
            {
                Id = _account.Id,
                AccountNumber = "01234"
            };

            _service.Update(target);

            // Assert
            var trace = _context.GetTracingService().DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Update", trace);
            Assert.Contains("Stage: 20", trace);
            Assert.Contains("Mode: 0", trace);
            Assert.Contains($"Entity Logical Name: {Account.EntityLogicalName}", trace);
            Assert.Contains($"Entity ID: {_account.Id}", trace);
        }

        [Fact]
        public void Should_not_trigger_plugin_for_filtered_attribute_if_attribute_is_not_present()
        {
            _context.Initialize(_account);
            _context.RegisterPluginStep<ValidatePipelinePlugin, Account>("Update", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous, filteringAttributes: new string[] { "name" });

            // Act
            var target = new Account
            {
                Id = _account.Id,
                AccountNumber = "01234"
            };

            _service.Update(target);

            // Assert
            var traces = _context.GetTracingService().DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Empty(traces);
        }

    }
}
