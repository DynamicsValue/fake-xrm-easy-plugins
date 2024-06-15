using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Pipeline.Scope
{
    /// <summary>
    /// Event Pipeline Scope extensions
    /// </summary>
    internal static class EventPipelineScopeOrganizationServiceExtensions
    {
        /// <summary>
        /// Returns the current event pipeline scope in which the service is being executed
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static EventPipelineScope GetEventPipelineScope(this IOrganizationService service)
        {
            var pipelineService = service as IPipelineOrganizationService;
            if (pipelineService == null)
            {
                return null;
            }

            return pipelineService.Scope;
        }
    }
}