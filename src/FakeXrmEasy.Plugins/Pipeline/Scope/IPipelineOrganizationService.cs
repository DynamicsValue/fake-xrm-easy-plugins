using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Pipeline.Scope
{
    /// <summary>
    /// A pipeline organization service is an IOrganizationService that is being executed as part of a specific EventPipelineScope
    /// </summary>
    internal interface IPipelineOrganizationService: IOrganizationService
    {
        /// <summary>
        /// The current event / execution scope 
        /// </summary>
        EventPipelineScope Scope { get; set; }
    }
}