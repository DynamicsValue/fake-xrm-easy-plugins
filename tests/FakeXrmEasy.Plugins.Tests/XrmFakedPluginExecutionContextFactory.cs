namespace FakeXrmEasy.Plugins.Tests
{
    public class XrmFakedPluginExecutionContextFactory : IXrmFakedPluginExecutionContextFactory
    {
        private XrmFakedPluginExecutionContext _default;

        public XrmFakedPluginExecutionContextFactory(XrmFakedPluginExecutionContext defaultContextProperties)
        {
            _default = defaultContextProperties;
        }

        public XrmFakedPluginExecutionContext New()
        {
            var plugCtx = XrmFakedPluginExecutionContext.New(); //FXE's default settings

            plugCtx.OrganizationId = _default.OrganizationId; //Your own default settings

            return plugCtx;
        }
    }
}