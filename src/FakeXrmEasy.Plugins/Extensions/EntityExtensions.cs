

using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;

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

            foreach(var key in entity1.Attributes.Keys)
            {
                if(entity2.Attributes.ContainsKey(key))
                {
                    clone[key] = entity2[key];
                }
            }

            return clone;
        }
    }
}
