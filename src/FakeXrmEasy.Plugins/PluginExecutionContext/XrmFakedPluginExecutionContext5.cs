#if FAKE_XRM_EASY_9
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Plugins.PluginExecutionContext
{
    /// <summary>
    /// XrmFakedPluginExecutionContext5 adds the necessary plugin context properties to know the calling client
    /// </summary>
    public class XrmFakedPluginExecutionContext5: XrmFakedPluginExecutionContext4, IPluginExecutionContext5
    {
        /// <summary>
        /// Gets or sets the initiating UserAgent, the client that sent the request (browser, model driven app, 
        /// </summary>
        public string InitiatingUserAgent { get; set; }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public XrmFakedPluginExecutionContext5()
        {
            InitiatingUserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36";
        }
    }
}
#endif