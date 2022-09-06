using FakeXrmEasy.Plugins.PluginSteps.PluginStepRegistrationFieldNames;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Linq;

namespace FakeXrmEasy.Plugins.PluginSteps.Extensions
{
    internal static class EntityExtensions
    {
        internal static int? GetMessageFilterPrimaryObjectCode(this Entity messageFilterEntity)
        {
            if(messageFilterEntity == null)
            {
                return new int?();
            }

            return messageFilterEntity[SdkMessageFilterFieldNames.PrimaryObjectTypeCode] != null ?
                     (int)messageFilterEntity[SdkMessageFilterFieldNames.PrimaryObjectTypeCode]
                     : new int?();
        }

        internal static string GetMessageFilterEntityLogicalName(this Entity messageFilterEntity)
        {
            if (messageFilterEntity == null)
            {
                return null;
            }

            return messageFilterEntity[SdkMessageFilterFieldNames.EntityLogicalName] != null ?
                     (string)messageFilterEntity[SdkMessageFilterFieldNames.EntityLogicalName]
                     : null;
        }

        internal static List<string> GetPluginStepFilteringAttributes(this Entity sdkMessageProcessingStep)
        {
            return !string.IsNullOrEmpty(sdkMessageProcessingStep.GetAttributeValue<string>(SdkMessageProcessingStepFieldNames.FilteringAttributes))
                            ? sdkMessageProcessingStep.GetAttributeValue<string>(SdkMessageProcessingStepFieldNames.FilteringAttributes)
                                .ToLowerInvariant()
                                .Split(',')
                                .ToList()
                            : new List<string>();
        }
    }
}
