#if FAKE_XRM_EASY_9
using System;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Plugins.PluginExecutionContext
{
    /// <summary>
    /// XrmFakedPluginExecutionContext6 adds the necessary plugin context properties to know the environment details
    /// </summary>
    public class XrmFakedPluginExecutionContext6: XrmFakedPluginExecutionContext5, IPluginExecutionContext6
    {
        /// <summary>
        /// The EnvironmentId
        /// </summary>
        public string EnvironmentId { get; set;  }
        
        /// <summary>
        /// The tenant Id
        /// </summary>
        public Guid TenantId { get; set;  }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public XrmFakedPluginExecutionContext6()
        {
            TenantId = Guid.NewGuid();
            EnvironmentId = Guid.NewGuid().ToString();
        }

        
    }
}
#endif