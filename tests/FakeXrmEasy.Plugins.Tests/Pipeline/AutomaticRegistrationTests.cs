using Crm;
using FakeXrmEasy.Plugins.PluginSteps.PluginStepRegistrationFieldNames;
using FakeXrmEasy.Tests.PluginsForTesting;
using System.Linq;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline
{
    public class AutomaticRegistrationTests: FakeXrmEasyAutomaticPipelineTestsBase
    {
        [Fact]
        public void Should_register_plugin_step_for_followup_plugin()
        {
            var followupPluginType = _context.CreateQuery(PluginStepRegistrationEntityNames.PluginType)
                            .Where(pluginType => pluginType[PluginTypeFieldNames.TypeName].Equals(typeof(FollowupPlugin).FullName))
                            .FirstOrDefault();

            Assert.NotNull(followupPluginType);
        }
    }
}
