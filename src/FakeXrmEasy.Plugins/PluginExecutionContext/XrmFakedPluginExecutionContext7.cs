#if FAKE_XRM_EASY_9
using System;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Plugins.PluginExecutionContext
{
    /// <summary>
    /// XrmFakedPluginExecutionContext7 adds a flag to know if the request came from an ApplicationUser or not
    /// </summary>
    public class XrmFakedPluginExecutionContext7: XrmFakedPluginExecutionContext6, IPluginExecutionContext7
    {
        /// <summary>
        /// Flag that determines if the request was initiated by an application user
        /// </summary>
        public bool IsApplicationUser { get; set;  }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public XrmFakedPluginExecutionContext7()
        {
            IsApplicationUser = false;
        }
        
    }
}
#endif