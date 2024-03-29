#if FAKE_XRM_EASY_9
using System;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Plugins.PluginExecutionContext
{
    /// <summary>
    /// Implements IPluginExecutionContext2 interface, an extension of IPluginExecutionContext adding support for azure AAD properties
    /// </summary>
    public class XrmFakedPluginExecutionContext3: XrmFakedPluginExecutionContext2, IPluginExecutionContext3
    {
        /// <summary>
        /// Gets id of the authenticated user
        /// </summary>
        public Guid AuthenticatedUserId { get; }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public XrmFakedPluginExecutionContext3()
        {
            AuthenticatedUserId = Guid.NewGuid();
        }

        
    }
}
#endif