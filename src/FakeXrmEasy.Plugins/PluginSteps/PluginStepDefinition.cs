using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Plugins.Definitions;
using FakeXrmEasy.Plugins.PluginImages;
using System;
using System.Collections.Generic;

namespace FakeXrmEasy.Plugins.PluginSteps
{
    /// <summary>
    /// Contains the properties of a plugin step that are needed to execute it
    /// </summary>
    public class PluginStepDefinition : IPluginStepDefinition
    {
        /// <summary>
        /// Primary key of the stored sdkmessageprocessingstep
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Stage where this plugins runs. If null, PostOperation will be used by default
        /// </summary>
        public ProcessingStepStage Stage { get; set; }

        /// <summary>
        /// Name of the OrganizationRequest that triggers this plugin step
        /// </summary>
        public string MessageName { get; set; }

        /// <summary>
        /// Mode of this plugin step. If null, Synchronous will be used by default
        /// </summary>
        public ProcessingStepMode Mode { get; set; }

        /// <summary>
        /// Entity Logical Name, the plugin will only execute for these entities. This property will take precedence over EntityTypeCode
        /// </summary>
        public string EntityLogicalName { get; set; }

        /// <summary>
        /// If present, the plugin will only execute for entities whose EntityTypeCode matches this value, or any entity otherwise
        /// EntityTypeCode will be ignored if EntityLogicalName is set
        /// </summary>
        public int? EntityTypeCode { get; set; }

        /// <summary>
        /// The order in which this plugin will run relative to other plugin steps of the same stage and mode
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// The attributes used to filter the execution of this plugin. The plugin will only execute if any of the attributes are present in the request,
        /// or will run regardless of any attributes if empty
        /// </summary>
        public IEnumerable<string> FilteringAttributes { get; set; }

        /// <summary>
        /// The name of the assembly that will be used to serch for the plugin type
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// Type of the plugin to be executed (the plugin class name)
        /// </summary>
        public string PluginType { get; set; }

        /// <summary>
        /// Any plugin images to be registered for this plugin step, or null or empty
        /// </summary>
        public IEnumerable<IPluginImageDefinition> ImagesDefinitions { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public PluginStepDefinition()
        {
            Stage = ProcessingStepStage.Postoperation;
            Mode = ProcessingStepMode.Synchronous;
        }
    }
}
