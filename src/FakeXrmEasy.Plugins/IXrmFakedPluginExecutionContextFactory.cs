namespace FakeXrmEasy.Plugins
{
    /// <summary>
    /// Factory method to create XrmFakedPluginExecutionContext instances with your own properties other than those set
    /// by FakeXrmEasy as part of pipeline simulation by default
    /// </summary>
    public interface IXrmFakedPluginExecutionContextFactory
    {
        /// <summary>
        /// Returns a brand-new instance of an XrmFakedPluginExecutionContext
        /// </summary>
        /// <returns></returns>
        XrmFakedPluginExecutionContext New();
    }
}