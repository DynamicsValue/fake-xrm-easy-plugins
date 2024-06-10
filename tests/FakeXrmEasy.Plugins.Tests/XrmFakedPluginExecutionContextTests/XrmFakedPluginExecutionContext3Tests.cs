#if FAKE_XRM_EASY_9
using System;
using FakeXrmEasy.Plugins.PluginExecutionContext;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.XrmFakedPluginExecutionContextTests
{
    public class XrmFakedPluginExecutionContext3Tests
    {
        [Fact]
        public void Should_set_default_plugin_context_properties()
        {
            var plugCtx = new XrmFakedPluginExecutionContext3();

            Assert.NotEqual(Guid.Empty, plugCtx.AuthenticatedUserId);
        }
    }
}
#endif