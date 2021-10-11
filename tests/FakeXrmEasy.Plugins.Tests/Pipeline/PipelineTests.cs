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
        [Fact]
        public void When_context_is_initialised_pipeline_is_disabled_by_default()
        {
            Assert.False((_context as XrmFakedContext).UsePipelineSimulation);
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
            (_context as XrmFakedContext).UsePipelineSimulation = true;
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
            (_context as XrmFakedContext).UsePipelineSimulation = true;
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
            (_context as XrmFakedContext).UsePipelineSimulation = true;
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
            (_context as XrmFakedContext).UsePipelineSimulation = true;
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
            (_context as XrmFakedContext).UsePipelineSimulation = true;
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
            (_context as XrmFakedContext).UsePipelineSimulation = true;
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
            (_context as XrmFakedContext).UsePipelineSimulation = true;
            var context = _context as XrmFakedContext;

            var target = new Account
            {
                Id = Guid.NewGuid(),
                Name = "Original"
            };

            context.RegisterPluginStep<PostOperationUpdatePlugin>("Create");
            IOrganizationService serivce = context.GetOrganizationService();

            serivce.Create(target);

            var updatedAccount = serivce.Retrieve(Account.EntityLogicalName, target.Id, new ColumnSet(true)).ToEntity<Account>();

            Assert.Equal("Updated", updatedAccount.Name);
        }

    }
}
