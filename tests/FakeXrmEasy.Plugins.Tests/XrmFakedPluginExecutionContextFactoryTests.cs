using System;
using FakeXrmEasy.Plugins.PluginExecutionContext;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests
{
    public class XrmFakedPluginExecutionContextFactoryTests: FakeXrmEasyTestsBase
    {
        private readonly IXrmFakedPluginExecutionContextFactory _factory;
        private Guid _organizationId;
        
        public XrmFakedPluginExecutionContextFactoryTests()
        {
            _organizationId = Guid.NewGuid();
            
            var customPluginContext =  XrmFakedPluginExecutionContext.New();
            customPluginContext.OrganizationId = _organizationId;
            
            _factory = new XrmFakedPluginExecutionContextFactory(customPluginContext);
        }

        [Fact]
        public void Should_return_custom_plugin_context_property()
        {
            var plugCtx = _factory.New();
            Assert.Equal(_organizationId, plugCtx.OrganizationId);
        }
    }
}