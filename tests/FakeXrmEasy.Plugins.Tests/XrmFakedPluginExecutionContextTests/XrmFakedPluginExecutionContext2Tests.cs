using FakeXrmEasy.Plugins.PluginExecutionContext;
using System;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.XrmFakedPluginExecutionContextTests
{
    public class XrmFakedPluginExecutionContext2Tests
    {
        [Fact]
        public void Should_set_default_plugin_context_properties()
        {
            var plugCtx = new XrmFakedPluginExecutionContext2();

            Assert.NotEqual(Guid.Empty, plugCtx.UserAzureActiveDirectoryObjectId);
            Assert.NotEqual(Guid.Empty, plugCtx.InitiatingUserAzureActiveDirectoryObjectId);
            
            Assert.Equal(Guid.Empty, plugCtx.InitiatingUserApplicationId);
            
            Assert.False(plugCtx.IsPortalsClientCall);
            Assert.Equal(Guid.Empty, plugCtx.PortalsContactId);
        }
    }
}