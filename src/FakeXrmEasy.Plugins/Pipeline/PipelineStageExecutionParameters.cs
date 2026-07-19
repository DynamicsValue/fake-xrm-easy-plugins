using System;
using System.Collections.Generic;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline.Scope;
using FakeXrmEasy.Plugins;
using FakeXrmEasy.Plugins.Extensions;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Pipeline
{
    internal class PipelineStageExecutionParameters
    {
        /// <summary>
        /// The event pipeline stage
        /// </summary>
        internal ProcessingStepStage Stage { get; set; }
        
        /// <summary>
        /// The event pipeline mode
        /// </summary>
        internal ProcessingStepMode Mode { get; set; }

        /// <summary>
        /// Original request that triggered this pipeline execution stage
        /// </summary>
        internal OrganizationRequest Request { get; set; }

        /// <summary>
        /// Original response of the request that triggered this pipeline execution stage
        /// </summary>
        internal OrganizationResponse Response { get; set; }

        /// <summary>
        /// Snapshot of the entity values before the execution of a non-bulk operation request
        /// </summary>
        internal Entity PreEntitySnapshot { get; set; }

        /// <summary>
        /// Snapshot of the entity values after the execution of a non-bulk operation request
        /// </summary>
        internal Entity PostEntitySnapshot { get; set; }
        
        /// <summary>
        /// Snapshot of all the entity preimages before the execution of a bulk operation request
        /// </summary>
        internal List<Entity> PreEntitySnapshotCollection { get; set; }
        
        /// <summary>
        /// Snapshot of all the entity postimages after the execution of a bulk operation request
        /// </summary>
        internal List<Entity> PostEntitySnapshotCollection { get; set; }

        /// <summary>
        /// The current event pipeline scope
        /// </summary>
        internal EventPipelineScope Scope { get; set; }
        
        /// <summary>
        /// The primary entity name of this pipeline execution
        /// </summary>
        internal string PrimaryEntityName { get; set; }
        
        /// <summary>
        /// The primary entity id of this pipeline execution
        /// </summary>
        internal Guid PrimaryEntityId { get; set; }
        
        /// <summary>
        /// Converts the current bulk operation pipeline request parameters into an array of multiple non-bulk operation pipeline execution parameters 
        /// </summary>
        /// <returns></returns>
        internal PipelineStageExecutionParameters[] ToNonBulkPipelineExecutionParameters()
        {
            var requests = Request.ToNonBulkOrganizationRequests();
            var pipelineParameters = new List<PipelineStageExecutionParameters>();
            foreach (var request in requests)
            {
                pipelineParameters.Add(new PipelineStageExecutionParameters()
                {
                    Stage = Stage,
                    Mode = Mode,
                    Request = request
                });
            }

            return pipelineParameters.ToArray();
        }
        
        /// <summary>
        /// Converts the current non-bulk operation pipeline request parameter into another pipeline execution parameter with a bulk operation and a single record
        /// </summary>
        /// <returns></returns>
        internal PipelineStageExecutionParameters ToBulkPipelineExecutionParameters()
        {
            var request = Request.ToBulkOrganizationRequest();
            return new PipelineStageExecutionParameters()
            {
                Stage = Stage,
                Mode = Mode,
                Request = request
            };
        }

        /// <summary>
        /// Creates new pipeline execution parameters from the current organization request
        /// </summary>
        /// <returns></returns>
        internal static PipelineStageExecutionParameters FromOrganizationRequest(OrganizationRequest request)
        {
            EventPipelineScope scope = null;
            
            var pipelineOrganizationRequest = request as PipelineOrganizationRequest;
            if (pipelineOrganizationRequest != null)
            {
                scope = pipelineOrganizationRequest.CurrentScope;
            }

            var organizationRequest = pipelineOrganizationRequest != null
                ? pipelineOrganizationRequest.OriginalRequest
                : request;
            
            var pipelineExecutionParameters = new PipelineStageExecutionParameters()
            {
                Request = organizationRequest,
                Scope = scope
            };

            PopulatePrimaryEntityParameters(pipelineExecutionParameters);
            return pipelineExecutionParameters;
        }

        private static void PopulatePrimaryEntityParameters(PipelineStageExecutionParameters parameters)
        {
            var inputParams = parameters.Request.Parameters;
            if (inputParams.ContainsKey("Target"))
            {
                var targetEntity = inputParams["Target"] as Entity;
                if (targetEntity != null)
                {
                    parameters.PrimaryEntityName = targetEntity.LogicalName;
                    parameters.PrimaryEntityId = targetEntity.Id;
                    
                    if (parameters.Request.IsCreateRequest() && targetEntity.Id == Guid.Empty)
                    {
                        parameters.PrimaryEntityId = Guid.NewGuid();
                    }
                }
            }
        }
    }
}
