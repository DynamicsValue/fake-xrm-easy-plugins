using Crm;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System.Linq;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.IXrmFakedContextPipelineExtensions
{
    public class GetStepsForStageWithRetrieveMultipleTests : FakeXrmEasyTestsBase
    {
        private readonly CreateRequest _createRequest;
        private readonly Account _target;

        public GetStepsForStageWithRetrieveMultipleTests()
        {
            _target = new Account() { };

            _createRequest = new CreateRequest()
            {
                Target = _target
            };
        }

        [Fact]
        public void Should_return_empty_list_of_steps_if_none_were_registered()
        {
            var steps = _context.GetPluginStepsForOrganizationRequestWithRetrieveMultiple(_createRequest.RequestName, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous, _createRequest);
            Assert.Empty(steps);
        }

        [Theory]
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_registered_plugin_step_for_exact_request_name_stage_and_mode(string requestName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<AccountNumberPlugin>(requestName, stage, mode);

            var steps = _context.GetPluginStepsForOrganizationRequestWithRetrieveMultiple(requestName, stage, mode, _createRequest);
            Assert.Single(steps);

            var pluginStep = steps.FirstOrDefault();
            Assert.Equal((int) stage, (pluginStep["stage"] as OptionSetValue).Value);
            Assert.Equal((int) mode, (pluginStep["mode"] as OptionSetValue).Value);
            Assert.Equal(requestName, (pluginStep["sdkmessage.name"] as AliasedValue).Value);

            var pluginType = typeof(AccountNumberPlugin);
            Assert.Equal(pluginType.Assembly.GetName().Name, (pluginStep["plugintype.assemblyname"] as AliasedValue).Value);
            Assert.Equal(pluginType.FullName, (pluginStep["plugintype.typename"] as AliasedValue).Value);

            Assert.False(pluginStep.Contains("filteringattributes"));
            Assert.False(pluginStep.Contains("rank"));
        }

        [Theory]
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_registered_plugin_step_for_exact_request_name_stage_and_mode_and_entity_type_code_if_present(string requestName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<AccountNumberPlugin>(requestName, stage, mode, primaryEntityTypeCode: Account.EntityTypeCode);

            var steps = _context.GetPluginStepsForOrganizationRequestWithRetrieveMultiple(requestName, stage, mode, _createRequest);
            Assert.Single(steps);

            var pluginStep = steps.FirstOrDefault();
            Assert.Equal((int)stage, (pluginStep["stage"] as OptionSetValue).Value);
            Assert.Equal((int)mode, (pluginStep["mode"] as OptionSetValue).Value);
            Assert.Equal(requestName, (pluginStep["sdkmessage.name"] as AliasedValue).Value);

            var pluginType = typeof(AccountNumberPlugin);
            Assert.Equal(pluginType.Assembly.GetName().Name, (pluginStep["plugintype.assemblyname"] as AliasedValue).Value);
            Assert.Equal(pluginType.FullName, (pluginStep["plugintype.typename"] as AliasedValue).Value);
            Assert.Equal(Account.EntityTypeCode, (pluginStep["sdkmessagefilter.primaryobjecttypecode"] as AliasedValue).Value);

            Assert.False(pluginStep.Contains("filteringattributes"));
            Assert.False(pluginStep.Contains("rank"));
        }

        [Theory]
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_not_return_registered_plugin_step_for_another_request_name(string requestName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<AccountNumberPlugin>("Update", stage, mode);

            var steps = _context.GetPluginStepsForOrganizationRequestWithRetrieveMultiple(requestName, stage, mode, _createRequest);
            Assert.Empty(steps);
        }

        [Theory]
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_multiple_registered_plugin_steps_for_exact_request_name_stage_and_mode_ordered_by_rank(string requestName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<FollowupPlugin2>(requestName, stage, mode, rank: 2);
            _context.RegisterPluginStep<FollowupPlugin>(requestName, stage, mode, rank: 1);

            var steps = _context.GetPluginStepsForOrganizationRequestWithRetrieveMultiple(requestName, stage, mode, _createRequest).ToList();
            Assert.Equal(2, steps.Count);

            var firstPluginStep = steps[0];
            Assert.Equal((int)stage, (firstPluginStep["stage"] as OptionSetValue).Value);
            Assert.Equal((int)mode, (firstPluginStep["mode"] as OptionSetValue).Value);
            Assert.Equal(requestName, (firstPluginStep["sdkmessage.name"] as AliasedValue).Value);

            var firstPluginType = typeof(FollowupPlugin);
            Assert.Equal(firstPluginType.Assembly.GetName().Name, (firstPluginStep["plugintype.assemblyname"] as AliasedValue).Value);
            Assert.Equal(firstPluginType.FullName, (firstPluginStep["plugintype.typename"] as AliasedValue).Value);

            var secondPluginStep = steps[1];
            Assert.Equal((int)stage, (secondPluginStep["stage"] as OptionSetValue).Value);
            Assert.Equal((int)mode, (secondPluginStep["mode"] as OptionSetValue).Value);
            Assert.Equal(requestName, (secondPluginStep["sdkmessage.name"] as AliasedValue).Value);

            var secondPluginType = typeof(FollowupPlugin2);
            Assert.Equal(secondPluginType.Assembly.GetName().Name, (secondPluginStep["plugintype.assemblyname"] as AliasedValue).Value);
            Assert.Equal(secondPluginType.FullName, (secondPluginStep["plugintype.typename"] as AliasedValue).Value);
        }

        [Theory]
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_registered_plugin_step_for_exact_request_name_stage_and_mode_with_filtering_attributes(string requestName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<AccountNumberPlugin>(requestName, stage, mode, filteringAttributes: new string[] { "name" });

            _target.Name = "Some name";

            var steps = _context.GetPluginStepsForOrganizationRequestWithRetrieveMultiple(requestName, stage, mode, _createRequest);
            Assert.Single(steps);

            var pluginStep = steps.FirstOrDefault();
            Assert.Equal((int)stage, (pluginStep["stage"] as OptionSetValue).Value);
            Assert.Equal((int)mode, (pluginStep["mode"] as OptionSetValue).Value);
            Assert.Equal(requestName, (pluginStep["sdkmessage.name"] as AliasedValue).Value);

            var pluginType = typeof(AccountNumberPlugin);
            Assert.Equal(pluginType.Assembly.GetName().Name, (pluginStep["plugintype.assemblyname"] as AliasedValue).Value);
            Assert.Equal(pluginType.FullName, (pluginStep["plugintype.typename"] as AliasedValue).Value);

            Assert.True(pluginStep.Contains("filteringattributes"));
            Assert.Equal("name", pluginStep["filteringattributes"]);
        }

        [Theory]
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_return_registered_plugin_step_for_exact_request_name_stage_and_mode_with_multiple_filtering_attributes(string requestName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<AccountNumberPlugin>(requestName, stage, mode, filteringAttributes: new string[] { "name", "accountcategorycode" });

            _target.AccountCategoryCode = new OptionSetValue(0);

            var steps = _context.GetPluginStepsForOrganizationRequestWithRetrieveMultiple(requestName, stage, mode, _createRequest);
            Assert.Single(steps);

            var pluginStep = steps.FirstOrDefault();
            Assert.Equal((int)stage, (pluginStep["stage"] as OptionSetValue).Value);
            Assert.Equal((int)mode, (pluginStep["mode"] as OptionSetValue).Value);
            Assert.Equal(requestName, (pluginStep["sdkmessage.name"] as AliasedValue).Value);

            var pluginType = typeof(AccountNumberPlugin);
            Assert.Equal(pluginType.Assembly.GetName().Name, (pluginStep["plugintype.assemblyname"] as AliasedValue).Value);
            Assert.Equal(pluginType.FullName, (pluginStep["plugintype.typename"] as AliasedValue).Value);

            Assert.True(pluginStep.Contains("filteringattributes"));
            Assert.Equal("name,accountcategorycode", pluginStep["filteringattributes"]);
        }

        [Theory]
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Prevalidation, ProcessingStepMode.Asynchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_not_return_registered_plugin_step_with_filtering_attributes_if_request_does_not_contain_such_attribute(string requestName, ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<AccountNumberPlugin>(requestName, stage, mode, filteringAttributes: new string[] { "name" });

            var steps = _context.GetPluginStepsForOrganizationRequestWithRetrieveMultiple(requestName, stage, mode, _createRequest);
            Assert.Empty(steps);
        }


    }
}
