
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Linq;

namespace FakeXrmEasy.Plugins.Extensions
{
    /// <summary>
    /// Extension methods to Entities needed for Plugins 
    /// 
    /// </summary>
    public static class EntityExtensions 
    {
        /// <summary>
        /// Gets a list of filtering attributes from the internal attributes property in the plugin step entity (ths was introduced due to expression tree limitations on the right hand side of the expression (error CS0854))
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public static List<string> GetPluginStepFilteringAttributes(this Entity step)
        {
            return !string.IsNullOrEmpty(step.GetAttributeValue<string>("filteringattributes")) ? step.GetAttributeValue<string>("filteringattributes").ToLowerInvariant().Split(',').ToList() : new List<string>();
        }
    }
}