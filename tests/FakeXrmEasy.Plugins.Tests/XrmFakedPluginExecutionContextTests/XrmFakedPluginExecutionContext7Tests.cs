#if FAKE_XRM_EASY_9
using FakeXrmEasy.Plugins.PluginExecutionContext;
using System;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.XrmFakedPluginExecutionContextTests
{
    public class XrmFakedPluginExecutionContext7Tests
    {
        [Fact]
        public void Should_set_default_plugin_context_properties()
        {
            var plugCtx = new XrmFakedPluginExecutionContext7();

            Assert.False(plugCtx.IsApplicationUser);
        }
    }
}
#endif