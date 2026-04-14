using System.Linq;
using DataverseEntities;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.Audit;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.Tests.PluginsForTesting;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline
{
    public class DefaultValuesInPipelineTests: FakeXrmEasyPipelineWithAuditAndMessagesTestsBase
    {
        [Fact]
        public void Should_not_pass_default_boolean_values_in_prevalidation_step_if_metadata_is_not_initialised()
        {
            _context.RegisterPluginStep<TracerPlugin>(new PluginStepDefinition()
            {
                EntityLogicalName = dv_test.EntityLogicalName,
                MessageName = "Create",
                Stage = ProcessingStepStage.Prevalidation,
                Mode = ProcessingStepMode.Synchronous,
            });

            _service.Create(new dv_test() { });
            
            var executedPluginSteps = _context.GetPluginStepAudit().CreateQuery().ToList();
            Assert.Single(executedPluginSteps);

            var target = executedPluginSteps.First().TargetEntity as dv_test;
            Assert.Null(target.dv_bool);
        }
        
        [Fact]
        public void Should_pass_default_boolean_values_in_prevalidation_step()
        {
            _context.InitializeMetadata(typeof(dv_test).Assembly);
            
            _context.RegisterPluginStep<TracerPlugin>(new PluginStepDefinition()
            {
                EntityLogicalName = dv_test.EntityLogicalName,
                MessageName = "Create",
                Stage = ProcessingStepStage.Prevalidation,
                Mode = ProcessingStepMode.Synchronous,
            });

            _service.Create(new dv_test() { });
            
            var executedPluginSteps = _context.GetPluginStepAudit().CreateQuery().ToList();
            Assert.Single(executedPluginSteps);

            var target = executedPluginSteps.First().TargetEntity as dv_test;
            Assert.False(target.dv_bool);
        }
        
        [Fact]
        public void Should_not_pass_default_boolean_values_in_preoperation_step_if_metadata_is_not_initialised()
        {
            _context.RegisterPluginStep<TracerPlugin>(new PluginStepDefinition()
            {
                EntityLogicalName = dv_test.EntityLogicalName,
                MessageName = "Create",
                Stage = ProcessingStepStage.Preoperation,
                Mode = ProcessingStepMode.Synchronous,
            });

            _service.Create(new dv_test() { });
            
            var executedPluginSteps = _context.GetPluginStepAudit().CreateQuery().ToList();
            Assert.Single(executedPluginSteps);

            var target = executedPluginSteps.First().TargetEntity as dv_test;
            Assert.Null(target.dv_bool);
        }
        
        [Fact]
        public void Should_not_pass_default_boolean_values_in_postoperation_step_if_metadata_is_not_initialised()
        {
            _context.RegisterPluginStep<TracerPlugin>(new PluginStepDefinition()
            {
                EntityLogicalName = dv_test.EntityLogicalName,
                MessageName = "Create",
                Stage = ProcessingStepStage.Postoperation,
                Mode = ProcessingStepMode.Synchronous,
            });

            _service.Create(new dv_test() { });
            
            var executedPluginSteps = _context.GetPluginStepAudit().CreateQuery().ToList();
            Assert.Single(executedPluginSteps);

            var target = executedPluginSteps.First().TargetEntity as dv_test;
            Assert.Null(target.dv_bool);
        }
        
        [Fact]
        public void Should_pass_default_values_in_preoperation_step()
        {
            _context.InitializeMetadata(typeof(dv_test).Assembly);
            
            _context.RegisterPluginStep<TracerPlugin>(new PluginStepDefinition()
            {
                EntityLogicalName = dv_test.EntityLogicalName,
                MessageName = "Create",
                Stage = ProcessingStepStage.Preoperation,
                Mode = ProcessingStepMode.Synchronous,
            });

            var id = _service.Create(new dv_test() { });
            
            var executedPluginSteps = _context.GetPluginStepAudit().CreateQuery().ToList();
            Assert.Single(executedPluginSteps);

            var target = executedPluginSteps.First().TargetEntity as dv_test;
            Assert.False(target.dv_bool);
            Assert.Equal(id, target.Id);
            Assert.Equal(_context.CallerProperties.CallerId.Id, target.CreatedBy.Id);
            Assert.Equal(_context.CallerProperties.CallerId.LogicalName, target.CreatedBy.LogicalName);
            Assert.Equal(_context.CallerProperties.CallerId.Id, target.ModifiedBy.Id);
            Assert.Equal(_context.CallerProperties.CallerId.LogicalName, target.ModifiedBy.LogicalName);
            Assert.Equal(_context.CallerProperties.CallerId.Id, target.OwnerId.Id);
            Assert.Equal(_context.CallerProperties.CallerId.LogicalName, target.OwnerId.LogicalName);
            Assert.Equal(_context.CallerProperties.CallerId.Id, target.OwningUser.Id);
            Assert.Equal(_context.CallerProperties.CallerId.LogicalName, target.OwningUser.LogicalName);
        }
    }
}