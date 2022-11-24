using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;

namespace FakeXrmEasy.Plugins.PluginSteps.Extensions
{
    /// <summary>
    /// Some string extension methods
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns an organization request type from a message name
        /// </summary>
        /// <param name="messageName"></param>
        /// <returns></returns>
        public static Type ToOrganizationCrudRequestType(this string messageName)
        {
            if (string.IsNullOrEmpty(messageName))
                return typeof(OrganizationRequest);

            if (messageName.Equals(MessageNameConstants.Create))
                return typeof(CreateRequest);

            if (messageName.Equals(MessageNameConstants.Delete))
                return typeof(DeleteRequest);

            if (messageName.Equals(MessageNameConstants.Retrieve))
                return typeof(RetrieveRequest);

            if (messageName.Equals(MessageNameConstants.RetrieveMultiple))
                return typeof(RetrieveMultipleRequest);

            if (messageName.Equals(MessageNameConstants.Update))
                return typeof(UpdateRequest);


            return typeof(OrganizationRequest);
        }
    }
}
