

using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using System.Linq;

namespace FakeXrmEasy.Plugins.Extensions
{
    /// <summary>
    /// Entity extensions
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        /// Replaces existing attributes in entity1, with the values from entity2
        /// </summary>
        /// <param name="entity1"></param>
        /// <param name="entity2"></param>
        /// <returns></returns>
        public static Entity ReplaceAttributesWith(this Entity entity1, Entity entity2)
        {
            var clone = entity1.Clone();

            var intersectKeys = entity1.Attributes.Keys
                                        .Where(k => entity2.Attributes.ContainsKey(k));

            foreach (var key in intersectKeys)
            {
                clone[key] = entity2[key];
            }

            return clone;
        }
    }
}
