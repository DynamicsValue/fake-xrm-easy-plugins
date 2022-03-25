using FakeXrmEasy.Abstractions.Plugins.Enums;
using System.Collections.Generic;

namespace FakeXrmEasy.Plugins.Images
{
    /// <summary>
    /// Contains necessary info about a plugin image used in Pipeline Simulation
    /// 
    /// 
    /// </summary>
    public class PluginImageDefinition
    {
        /// <summary>
        /// Name of the plugin image definition
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Type of Image: PreImage, PostImage, or Both
        /// </summary>
        public ProcessingStepImageType ImageType { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> Attributes { get; private set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="imageType"></param>
        /// <param name="attributes"></param>
        public PluginImageDefinition(string name, ProcessingStepImageType imageType, params string[] attributes)
        {
            Name = name;
            ImageType = imageType;
            if (attributes != null && attributes.Length > 0)
            {
                Attributes = attributes;
            }
        }
    }
}
