namespace FakeXrmEasy.Plugins.PluginSteps.PluginStepRegistrationFieldNames
{
    internal class SdkMessageFieldNames
    {
        /// <summary>
        /// Property where the Name of the message will be stored
        /// </summary>
        internal const string Name = "name";
    }

    internal class SdkMessageFilterFieldNames
    {
        /// <summary>
        /// This property is a hack to support storing the logical name instead of the primary object type, it doesn't exist as an actual field in Dataverse
        /// </summary>
        internal const string EntityLogicalName = "entitylogicalname";

        /// <summary>
        /// Primary object type of the associated entity
        /// </summary>
        internal const string PrimaryObjectTypeCode = "primaryobjecttypecode";
    }

    internal class PluginTypeFieldNames
    {
        internal const string PluginTypeId = "plugintypeid";
        internal const string AssemblyName = "assemblyname";
        internal const string Name = "name";
        internal const string TypeName = "typename";
        internal const string Major = "major";
        internal const string Minor = "minor";
        internal const string Version = "version";
    }

    internal class SdkMessageProcessingStepFieldNames
    {
        /// <summary>
        /// Reference to the plugin assembly (PluginType)
        /// </summary>
        internal const string EventHandler = "eventhandler";

        /// <summary>
        /// Associated message (Create, Update, etc)
        /// </summary>
        internal const string SdkMessageId = "sdkmessageid";

        /// <summary>
        /// Associated filtering entity, if any
        /// </summary>
        internal const string SdkMessageFilterId = "sdkmessagefilterid";

        /// <summary>
        /// Filtering attributes 
        /// </summary>
        internal const string FilteringAttributes = "filteringattributes";

        /// <summary>
        /// Execution mode: sync or async
        /// </summary>
        internal const string Mode = "mode";

        /// <summary>
        /// Stage of the plugin execution: PreValidation, PreOperation, PostOperation
        /// </summary>
        internal const string Stage = "stage";

        /// <summary>
        /// Rank: sequence of execution
        /// </summary>
        internal const string Rank = "rank";
    }

    internal class SdkMessageProcessingStepImageFieldNames
    {
        /// <summary>
        /// Nam eof the image
        /// </summary>
        internal const string Name = "name";

        /// <summary>
        /// Associated plugin step
        /// </summary>
        internal const string SdkMessageProcessingStepId = "sdkmessageprocessingstepid";

        internal const string ImageType = "imagetype";

        internal const string Attributes = "attributes";
    }
}
