using FakeXrmEasy.Plugins.PluginExecutionContext;
using System;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.XrmFakedPluginExecutionContextTests
{
    public class XrmFakedPluginExecutionContext4Tests
    {
        [Fact]
        public void Should_set_default_plugin_context_properties()
        {
            var plugCtx = new XrmFakedPluginExecutionContext4();

            Assert.NotNull(plugCtx.PreEntityImagesCollection);
            Assert.NotNull(plugCtx.PostEntityImagesCollection);
        }
    }
}