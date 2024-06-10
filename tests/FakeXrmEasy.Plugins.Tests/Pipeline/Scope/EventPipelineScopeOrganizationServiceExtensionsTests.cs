using FakeXrmEasy.Pipeline.Scope;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline.Scope
{
    public class EventPipelineScopeOrganizationServiceExtensionsTests: FakeXrmEasyTestsBase
    {
        private IPipelineOrganizationService _pipelineOrganizationService;

        [Fact]
        public void Should_return_null_if_it_is_not_a_pipeline_organization_service()
        {
            Assert.Null(_service.GetEventPipelineScope());
        }
        
        [Fact]
        public void Should_return_null_if_is_a_pipeline_organization_service_without_a_scope_yet()
        {
            _pipelineOrganizationService = PipelineOrganizationServiceFactory.New(_service, null);
            Assert.Null(_pipelineOrganizationService.GetEventPipelineScope());
        }
        
        [Fact]
        public void Should_return_the_scope_of_a_pipeline_organization_service()
        {
            var scope = new EventPipelineScope();
            _pipelineOrganizationService = PipelineOrganizationServiceFactory.New(_service, scope);
            Assert.Equal(scope, _pipelineOrganizationService.GetEventPipelineScope());
        }
    }
}