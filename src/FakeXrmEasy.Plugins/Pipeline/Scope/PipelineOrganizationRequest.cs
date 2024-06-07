using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Pipeline.Scope
{
    /// <summary>
    /// An internal organization request that decorates additional properties for pipeline processing
    /// </summary>
    internal class PipelineOrganizationRequest
    {
        /// <summary>
        /// A reference to the the original request that is being executed
        /// </summary>
        public OrganizationRequest OriginalRequest { get; set; }
        
        /// <summary>
        /// Info about the current event pipeline scope
        /// </summary>
        public EventPipelineScope CurrentScope { get; set; }
    }
}