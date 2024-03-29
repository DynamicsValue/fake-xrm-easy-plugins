#if FAKE_XRM_EASY_9
using System;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Plugins.PluginExecutionContext
{
    /// <summary>
    /// Implements IPluginExecutionContext2 interface, an extension of IPluginExecutionContext adding support for azure AAD properties
    /// </summary>
    public class XrmFakedPluginExecutionContext2: XrmFakedPluginExecutionContext, IPluginExecutionContext2
    {
        /// <summary>
        /// Gets azure active directory object Id of user.
        /// </summary>
        public Guid UserAzureActiveDirectoryObjectId { get; set; }
        
        /// <summary>
        /// Gets azure active directory object Id of user that initiates web service.
        /// </summary>
        public Guid InitiatingUserAzureActiveDirectoryObjectId { get; set; }
        
        /// <summary>
        /// Gets application Id of user that initiates the plugin (for NON-app user .. it is Guid.Empty)
        /// </summary>
        public Guid InitiatingUserApplicationId { get; set; }
        
        /// <summary>
        /// Gets contactId that got passed for the calls that come from portals client to web service (for NON-portal/Anonymous call, it is guid.Empty)
        /// </summary>
        public Guid PortalsContactId { get; set; }
        
        /// <summary>
        /// Gets a value indicating whether 'True' if the call is originated from Portals client
        /// </summary>
        public bool IsPortalsClientCall { get; set;}
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public XrmFakedPluginExecutionContext2()
        {
            UserAzureActiveDirectoryObjectId = Guid.NewGuid();
            InitiatingUserAzureActiveDirectoryObjectId = Guid.NewGuid();
        }
    }
}
#endif