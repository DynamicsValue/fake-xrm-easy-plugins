using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Plugins.Tests.PluginSteps
{
    public class EarlyBoundEntityWithNoEntityTypeCode : Entity
    {
        public EarlyBoundEntityWithNoEntityTypeCode()
        {
            LogicalName = "Dummy";
        }
    }
}
