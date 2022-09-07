using FakeXrmEasy.Abstractions.Plugins.Enums;
using System.Collections.Generic;

namespace FakeXrmEasy.Plugins.Definitions
{
    /// <summary>
    ///  Contains necessary registration info of a plugin image used in Pipeline Simulation
    /// </summary>
    public interface IPluginImageDefinition
    {
        /// <summary>
        /// Name of the plugin image definition
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Type of Image: PreImage, PostImage, or Both
        /// </summary>
        ProcessingStepImageType ImageType { get; set; }

        /// <summary>
        /// Attributes that this plugin image contains
        /// </summary>
        IEnumerable<string> Attributes { get; set; }
    }
}
