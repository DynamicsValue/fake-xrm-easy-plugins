#if FAKE_XRM_EASY_9
using FakeXrmEasy.Plugins.PluginExecutionContext;
using System;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.XrmFakedPluginExecutionContextTests
{
    public class XrmFakedPluginExecutionContext6Tests
    {
        [Fact]
        public void Should_set_default_plugin_context_properties()
        {
            var plugCtx = new XrmFakedPluginExecutionContext6();

            Assert.NotEqual(Guid.Empty, plugCtx.TenantId);
            Assert.NotEqual(string.Empty, plugCtx.EnvironmentId);
        }
    }
}
#endif