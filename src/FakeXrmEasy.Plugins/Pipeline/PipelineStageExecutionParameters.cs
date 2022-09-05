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
        /// Entity is defined if an Entity came in the Target of the original request
        /// </summary>
        internal Entity Entity { get; set; }

        /// <summary>
        /// EntityReference is defined if an EntityReference came in the Target of the original request
        /// </summary>
        internal EntityReference EntityReference { get; set; }
        internal Entity PreviousValues { get; set; }
        internal Entity ResultingAttributes { get; set; }
    }
}
