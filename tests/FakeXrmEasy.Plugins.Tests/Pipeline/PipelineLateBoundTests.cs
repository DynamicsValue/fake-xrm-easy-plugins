using System;
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
    public class PipelineLateBoundTests: FakeXrmEasyPipelineTestsBase
    {
        private readonly Account _account;
        private readonly Contact _contact;

        public PipelineLateBoundTests()
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

            _contact = new Contact()
            {
                Id = Guid.NewGuid()
            };
        }

        [Fact]
        public void Should_trigger_registered_postoperation_plugin_step_but_not_persist_account_number_attribute_when_execute_plugin_with_is_called()
        {
            _context.RegisterPluginStep<AccountNumberPlugin>(Account.EntityLogicalName, "Create");

            _context.ExecutePluginWith<CreateAccountPlugin>();

            var account = _context.CreateQuery<Account>().FirstOrDefault();
            Assert.NotNull(account);
            Assert.False(account.Attributes.ContainsKey("accountnumber"));
        }

        [Fact]
        public void Should_not_trigger_registered_preoperation_plugin_step_if_it_was_registered_against_another_entity()
        {
            _context.RegisterPluginStep<AccountNumberPlugin>(Contact.EntityLogicalName, "Create", ProcessingStepStage.Preoperation);

            _context.ExecutePluginWith<CreateAccountPlugin>();

            var account = _context.CreateQuery<Account>().FirstOrDefault();
            Assert.NotNull(account);
            Assert.False(account.Attributes.ContainsKey("accountnumber"));
        }

        [Fact]
        public void Should_trigger_plugin_registered_on_sync_delete_preoperation()
        {
            // Arange
            _context.Initialize(_contact);
            _context.RegisterPluginStep<ValidatePipelinePlugin>(Contact.EntityLogicalName, "Delete", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous);

            // Act
            _service.Delete(Contact.EntityLogicalName, _contact.Id);

            // Assert
            var tracingService = _context.GetTracingService();
            var trace = tracingService.DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Delete", trace);
            Assert.Contains("Stage: 20", trace);
            Assert.Contains("Mode: 0", trace);
            Assert.Contains($"Entity Reference Logical Name: {Contact.EntityLogicalName}", trace);
            Assert.Contains($"Entity Reference ID: {_contact.Id}", trace);
        }

        [Fact]
        public void Should_trigger_plugin_registered_on_sync_delete_postoperation()
        {
            // Arange
            _context.Initialize(_contact);
            _context.RegisterPluginStep<ValidatePipelinePlugin>(Contact.EntityLogicalName, "Delete", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous);

            // Act
            _service.Delete(Contact.EntityLogicalName, _contact.Id);

            // Assert
            var tracingService = _context.GetTracingService();
            var trace = tracingService.DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Delete", trace);
            Assert.Contains("Stage: 40", trace);
            Assert.Contains("Mode: 0", trace);
            Assert.Contains($"Entity Reference Logical Name: {Contact.EntityLogicalName}", trace);
            Assert.Contains($"Entity Reference ID: {_contact.Id}", trace);
        }

        [Fact]
        public void When_PluginStepRegisteredAsDeletePostOperationAsyncronous_Expect_CorrectValues()
        {
            // Arange
            _context.Initialize(_contact);

            // Act
            _context.RegisterPluginStep<ValidatePipelinePlugin>(Contact.EntityLogicalName, "Delete", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous);

            _service.Delete(Contact.EntityLogicalName, _contact.Id);

            // Assert
            var tracingService = _context.GetTracingService();
            var trace = tracingService.DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Delete", trace);
            Assert.Contains("Stage: 40", trace);
            Assert.Contains("Mode: 1", trace);
            Assert.Contains($"Entity Reference Logical Name: {Contact.EntityLogicalName}", trace);
            Assert.Contains($"Entity Reference ID: {_contact.Id}", trace);
        }

        [Fact]
        public void Should_trigger_plugin_registered_on_sync_update_preoperation()
        {
            // Arange
            _context.Initialize(_contact);
            _context.RegisterPluginStep<ValidatePipelinePlugin>(Contact.EntityLogicalName, "Update", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous);

            // Act
            var updatedEntity = new Contact
            {
                Id = _contact.Id
            };

            _service.Update(updatedEntity);

            // Assert
            var trace = _context.GetTracingService().DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Update", trace);
            Assert.Contains("Stage: 20", trace);
            Assert.Contains("Mode: 0", trace);
            Assert.Contains($"Entity Logical Name: {Contact.EntityLogicalName}", trace);
            Assert.Contains($"Entity ID: {_contact.Id}", trace);
        }

        [Fact]
        public void Should_trigger_plugin_registered_on_sync_update_postoperation()
        {
            // Arange
            _context.Initialize(_contact);
            _context.RegisterPluginStep<ValidatePipelinePlugin>(Contact.EntityLogicalName, "Update", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous);

            // Act
            var updatedEntity = new Contact
            {
                Id = _contact.Id
            };

            _service.Update(updatedEntity);

            // Assert
            var trace = _context.GetTracingService().DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Update", trace);
            Assert.Contains("Stage: 40", trace);
            Assert.Contains("Mode: 0", trace);
            Assert.Contains($"Entity Logical Name: {Contact.EntityLogicalName}", trace);
            Assert.Contains($"Entity ID: {_contact.Id}", trace);
        }

        [Fact]
        public void Should_trigger_plugin_registered_on_async_update_postoperation()
        {
            // Arange
            _context.Initialize(_contact);
            _context.RegisterPluginStep<ValidatePipelinePlugin>(Contact.EntityLogicalName, "Update", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous);

            // Act
            var updatedEntity = new Contact
            {
                Id = _contact.Id
            };

            _service.Update(updatedEntity);

            // Assert
            var trace = _context.GetTracingService().DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Update", trace);
            Assert.Contains("Stage: 40", trace);
            Assert.Contains("Mode: 1", trace);
            Assert.Contains($"Entity Logical Name: {Contact.EntityLogicalName}", trace);
            Assert.Contains($"Entity ID: {_contact.Id}", trace);
        }
        
        [Fact]
        public void Should_trigger_plugin_in_the_pre_operation_stage_sync_when_plugin_step_is_registered()
        {
            // Arange
            _context.RegisterPluginStep<ValidatePipelinePlugin>(Contact.EntityLogicalName, "Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous);

            // Act
            _service.Create(_contact);

            // Assert
            var trace = _context.GetTracingService().DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Create", trace);
            Assert.Contains("Stage: 20", trace);
            Assert.Contains("Mode: 0", trace);
            Assert.Contains($"Entity Logical Name: {Contact.EntityLogicalName}", trace);
            Assert.Contains($"Entity ID: {_contact.Id}", trace);
        }

        [Fact]
        public void Should_trigger_plugin_in_the_post_operation_stage_sync_when_plugin_step_is_registered()
        {
            // Arange
            _context.RegisterPluginStep<ValidatePipelinePlugin>(Contact.EntityLogicalName, "Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous);

            // Act
            _service.Create(_contact);

            // Assert
            var trace = _context.GetTracingService().DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Create", trace);
            Assert.Contains("Stage: 40", trace);
            Assert.Contains("Mode: 0", trace);
            Assert.Contains($"Entity Logical Name: {Contact.EntityLogicalName}", trace);
            Assert.Contains($"Entity ID: {_contact.Id}", trace);
        }

        [Fact]
        public void Should_trigger_plugin_in_the_post_operation_stage_async_when_plugin_step_is_registered()
        {
            // Arange
            _context.RegisterPluginStep<ValidatePipelinePlugin>(Contact.EntityLogicalName, "Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous);

            // Act
            _service.Create(_contact);

            // Assert
            var trace = _context.GetTracingService().DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Create", trace);
            Assert.Contains("Stage: 40", trace);
            Assert.Contains("Mode: 1", trace);
            Assert.Contains($"Entity Logical Name: {Contact.EntityLogicalName}", trace);
            Assert.Contains($"Entity ID: {_contact.Id}", trace);
        }


        [Fact]
        public void Should_trigger_plugin_registered_on_sync_update_prevalidation()
        {
            // Arange
            _context.Initialize(_contact);
            _context.RegisterPluginStep<ValidatePipelinePlugin>(Contact.EntityLogicalName, "Update", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous);

            // Act
            var updatedEntity = new Contact
            {
                Id = _contact.Id
            };

            _service.Update(updatedEntity);

            // Assert
            var trace = _context.GetTracingService().DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Update", trace);
            Assert.Contains("Stage: 10", trace);
            Assert.Contains("Mode: 0", trace);
            Assert.Contains($"Entity Logical Name: {Contact.EntityLogicalName}", trace);
            Assert.Contains($"Entity ID: {_contact.Id}", trace);
        }

        [Fact]
        public void Should_trigger_plugin_registered_on_sync_create_prevalidation()
        {
            // Arange
            _context.RegisterPluginStep<ValidatePipelinePlugin>(Contact.EntityLogicalName, "Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous);

            // Act
            _service.Create(_contact);

            // Assert
            var trace = _context.GetTracingService().DumpTrace().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(5, trace.Length);
            Assert.Contains("Message Name: Create", trace);
            Assert.Contains("Stage: 10", trace);
            Assert.Contains("Mode: 0", trace);
            Assert.Contains($"Entity Logical Name: {Contact.EntityLogicalName}", trace);
            Assert.Contains($"Entity ID: {_contact.Id}", trace);
        }

        [Theory]
        [InlineData("accountnumber")]
        [InlineData("AccountNumber")]
        public void Should_trigger_plugin_for_filtered_attribute_if_a_registration_exists(string attributeName)
        {
            _context.Initialize(_account);
            _context.RegisterPluginStep<ValidatePipelinePlugin>(Account.EntityLogicalName, "Update", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous, filteringAttributes: new string[] { attributeName });

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
            _context.RegisterPluginStep<ValidatePipelinePlugin>(Account.EntityLogicalName, "Update", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous, filteringAttributes: new string[] { attributeName, "name" });

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
            _context.RegisterPluginStep<ValidatePipelinePlugin>(Account.EntityLogicalName, "Update", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous, filteringAttributes: new string[] { "name" });

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
