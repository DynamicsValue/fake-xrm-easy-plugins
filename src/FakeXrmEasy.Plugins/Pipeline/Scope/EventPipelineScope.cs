using FakeXrmEasy.Plugins;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Pipeline.Scope
{
    /// <summary>
    /// An event pipeline scope contains information about the current stack / context that is being executed for a given request
    /// It uses a different naming convention 'Scope' as opposed to 'Context'  to avoid confusion with existing plugin execution context naming conventions.
    /// Think of it as kind of a function 'scope', like a reference to a specific call stack
    /// </summary>
    internal class EventPipelineScope
    {
        /// <summary>
        /// The current plugin execution context that is being executed
        /// </summary>
        public XrmFakedPluginExecutionContext PluginContext { get; set; }
        
        /// <summary>
        /// Additional plugin context properties that we need to store internally in addition to IPluginExecutionContext properties
        /// </summary>
        public XrmFakedPluginContextProperties PluginContextProperties { get; set; }
    }
}