#if FAKE_XRM_EASY_9
using System.Collections.Generic;
using DataverseEntities;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline
{
    public class PipelineStageExecutionParametersTests
    {
        private readonly PipelineStageExecutionParameters _parameters;
        private readonly OrganizationRequest _bulkRequest;
        private readonly OrganizationRequest _nonBulkRequest;
        
        private readonly Account _account1;
        private readonly Account _account2;
        
        public PipelineStageExecutionParametersTests()
        {
            _account1 = new Account() { };
            _account2 = new Account() { };

            var entityCollection = new EntityCollection(new List<Entity>()
            {
                _account1, _account2
            });
            
            _bulkRequest = new CreateMultipleRequest()
            {
                Targets = entityCollection
            };

            _nonBulkRequest = new CreateRequest()
            {
                Target = _account1
            };
            
            _parameters = new PipelineStageExecutionParameters()
            {
                Request = _bulkRequest
            };
        }

        [Theory]
        [InlineData(ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_convert_bulk_operation_pipeline_stage_execution_parameters_into_an_array_of_non_bulk_pipeline_stage_execution_parameters
            (ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _parameters.Stage = stage;
            _parameters.Mode = mode;

            var pipelineParameters = _parameters.ToNonBulkPipelineExecutionParameters();
            Assert.NotNull(pipelineParameters);
            
            Assert.Equal(2, pipelineParameters.Length);

            AssertNonBulkPipelineParameters(pipelineParameters[0], stage, mode, _account1);
            AssertNonBulkPipelineParameters(pipelineParameters[1], stage, mode, _account2);
        }
        
        [Theory]
        [InlineData(ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_convert_non_bulk_operation_pipeline_stage_execution_parameters_into_a_bulk_pipeline_stage_execution_parameter_with_a_single_request
            (ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _parameters.Request = _nonBulkRequest;
            _parameters.Stage = stage;
            _parameters.Mode = mode;

            var pipelineParameters = _parameters.ToBulkPipelineExecutionParameters();
            Assert.NotNull(pipelineParameters);
            
            AssertBulkPipelineParameters(pipelineParameters, stage, mode, _account1);
        }

        private void AssertNonBulkPipelineParameters(PipelineStageExecutionParameters parameters, ProcessingStepStage stage,
            ProcessingStepMode mode,
            Entity record)
        {
            Assert.Equal(stage, parameters.Stage);
            Assert.Equal(mode, parameters.Mode);
            Assert.Equal(record, parameters.Request["Target"]);
        }
        
        private void AssertBulkPipelineParameters(PipelineStageExecutionParameters parameters, ProcessingStepStage stage,
            ProcessingStepMode mode,
            Entity record)
        {
            var entityCollection = parameters.Request["Targets"] as EntityCollection;
            
            Assert.Equal(stage, parameters.Stage);
            Assert.Equal(mode, parameters.Mode);
            Assert.Equal(record, entityCollection.Entities[0]);
        }
    }
}
#endif