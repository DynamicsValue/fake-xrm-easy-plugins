using System;
using System.Linq;
using DataverseEntities;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline.Scope
{
    public class PipelineScopeTests: FakeXrmEasyPipelineTestsBase
    {
        private readonly Account _account;
        private readonly Contact _contact;

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

            _contact = new Contact()
            {
                Id = Guid.NewGuid()
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
        
        
    }
}