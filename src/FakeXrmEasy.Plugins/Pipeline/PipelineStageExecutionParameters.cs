using System.Collections.Generic;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Pipeline
{
    internal class PipelineStageExecutionParameters
    {
        internal string RequestName { get; set; }
        internal ProcessingStepStage Stage { get; set; }
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
        internal List<Entity> PreEntitySnapshots { get; set; }
        
        /// <summary>
        /// Snapshot of all the entity postimages after the execution of a bulk operation request
        /// </summary>
        internal List<Entity> PostEntitySnapshots { get; set; }
    }
}
