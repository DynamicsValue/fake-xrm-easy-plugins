#if FAKE_XRM_EASY_9
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Plugins.PluginExecutionContext
{
    /// <summary>
    /// XrmFakedPluginExecutionContext4 adds the necessary plugin context properties for bulk operations (xMultiple messages)
    /// </summary>
    public class XrmFakedPluginExecutionContext4: XrmFakedPluginExecutionContext3, IPluginExecutionContext4
    {
        /// <summary>
        /// Contains a collection of PreEntityImages
        /// </summary>
        public EntityImageCollection[] PreEntityImagesCollection { get; set;  }
        
        /// <summary>
        /// Contains a collection of PostEntityImages
        /// </summary>
        public EntityImageCollection[] PostEntityImagesCollection { get; set;  }

        /// <summary>
        /// Default constructor
        /// </summary>
        public XrmFakedPluginExecutionContext4()
        {
            PreEntityImagesCollection = new EntityImageCollection[] {};
            PostEntityImagesCollection = new EntityImageCollection[] {};
        }
        
    }
}
#endif