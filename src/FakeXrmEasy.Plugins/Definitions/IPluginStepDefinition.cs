using FakeXrmEasy.Abstractions.Plugins.Enums;
using System;
using System.Collections.Generic;

namespace FakeXrmEasy.Plugins.Definitions
{
    /// <summary>
    /// Interface that contains all the necessary information to register a plugin step
    /// </summary>
    public interface IPluginStepDefinition
    {
        /// <summary>
        /// Primary key of the stored sdkmessageprocessingstep
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Stage where this plugins runs.
        /// </summary>
        ProcessingStepStage Stage { get; set; }

        /// <summary>
        /// Name of the OrganizationRequest that triggers this plugin step
        /// </summary>
        string MessageName { get; set; }

        /// <summary>
        /// Mode of this plugin step
        /// </summary>
        ProcessingStepMode Mode { get; set; }

        /// <summary>
        /// Entity Logical Name or null, the plugin will only execute for these entities if specified. 
        /// This property will take precedence over EntityTypeCode
        /// </summary>
        string EntityLogicalName { get; set; }

        /// <summary>
        /// If present, the plugin will only execute for entities whose EntityTypeCode matches this value, or any entity otherwise
        /// EntityTypeCode will be ignored if EntityLogicalName is set
        /// </summary>
        int? EntityTypeCode { get; set; }

        /// <summary>
        /// The order in which this plugin will run relative to other plugin steps of the same stage and mode
        /// </summary>
        int Rank { get; set; }

        /// <summary>
        /// The attributes used to filter the execution of this plugin. The plugin will only execute if any of the attributes are present in the request,
        /// or will run regardless of any attributes if empty
        /// </summary>
        IEnumerable<string> FilteringAttributes { get; set; }

        /// <summary>
        /// The name of the assembly where the plugin type will be searched and executed
        /// </summary>
        string AssemblyName { get; set; }

        /// <summary>
        /// Type of the plugin to be executed (the plugin class name)
        /// </summary>
        string PluginType { get; set; }

        /// <summary>
        /// Any plugin images to be registered for this plugin step, or null or empty
        /// </summary>
        IEnumerable<IPluginImageDefinition> ImagesDefinitions { get; set; }
    }
}
