using System;
using System.Linq;
using DataverseEntities;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.Audit;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline.Scope
{
    public class PipelineScopeTests: FakeXrmEasyPipelineWithAuditTestsBase
    {
        private readonly Account _account;

        public PipelineScopeTests()
        {
            _account = new Account()
            {
                Id = Guid.NewGuid(),
                AccountNumber = "1234567890",
                AccountCategoryCode = account_accountcategorycode.Standard,
                NumberOfEmployees = 5,
                Revenue = new Money(20000),
                Telephone1 = "+123456"
            };
        }
        
        
        [Fact]
        public void Should_throw_infinite_loop_exception_if_depth_reaches_a_value_higher_than_max_depth()
        {
            _context.Initialize(_account);
            
            _context.RegisterPluginStep<InfiniteUpdatePlugin>(new PluginStepDefinition()
            {
                MessageName = "Update",
                EntityLogicalName = DataverseEntitiesPartial.Account.EntityLogicalName,
                Stage = ProcessingStepStage.Postoperation,
                Mode = ProcessingStepMode.Synchronous
            });

            XAssert.ThrowsFaultCode(ErrorCodes.SdkCorrelationTokenDepthTooHigh,
                () => _service.Update(new Account() { Id = _account.Id, NumberOfEmployees = 6 }));
        }
        
        [Fact]
        public void Should_stop_executing_depth_plugin_on_the_2nd_call()
        {
            _context.Initialize(_account);
            
            _context.RegisterPluginStep<DepthPlugin>(new PluginStepDefinition()
            {
                MessageName = "Update",
                EntityLogicalName = DataverseEntitiesPartial.Account.EntityLogicalName,
                Stage = ProcessingStepStage.Postoperation,
                Mode = ProcessingStepMode.Synchronous
            });

            _service.Update(new Account() { Id = _account.Id, NumberOfEmployees = 6 });

            var auditedSteps = _context.GetPluginStepAudit().CreateQuery().ToList();
            Assert.Equal(2, auditedSteps.Count);
            
            //Inner plugins are logged first as they are called recursively and auditing is saved post plugin execution
            //Hence why Depth is recorded in reverse order
            Assert.Equal(2, auditedSteps[0].PluginContext.Depth);
            Assert.Equal(1, auditedSteps[1].PluginContext.Depth);
        }
    }
}