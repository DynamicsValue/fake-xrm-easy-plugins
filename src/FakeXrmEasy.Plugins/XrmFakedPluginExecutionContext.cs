using Microsoft.Xrm.Sdk;
using System;
using System.Runtime.Serialization;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using FakeXrmEasy.Plugins.PluginExecutionContext;

namespace FakeXrmEasy.Plugins
{
    /// <summary>
    /// Holds custom properties of a IPluginExecutionContext
    /// Extracted from https://msdn.microsoft.com/es-es/library/microsoft.xrm.sdk.ipluginexecutioncontext_properties.aspx
    /// </summary>
    [DataContract(Name = "PluginExecutionContext", Namespace = "")]
    public class XrmFakedPluginExecutionContext : IPluginExecutionContext
    {
        /// <summary>
        /// Gets the GUID of the business unit that the user making the request, also known as the calling user, belongs to.
        /// </summary>
        [DataMember(Order = 1)]
        public Guid BusinessUnitId { get; set; }

        /// <summary>
        /// Gets the GUID for tracking plug-in or custom workflow activity execution.
        /// </summary>
        [DataMember(Order = 2)]
        public Guid CorrelationId { get; set; }

        /// <summary>
        /// Gets the current depth of execution in the call stack.
        /// </summary>
        [DataMember(Order = 3)]
        public int Depth { get; set; }

        /// <summary>
        /// Gets the GUID of the system user account under which the current pipeline is executing.
        /// </summary>
        [DataMember(Order = 4)]
        public Guid InitiatingUserId { get; set; }

        /// <summary>
        /// Gets the parameters of the request message that triggered the event that caused the plug-in to execute.
        /// </summary>
        [DataMember(Order = 5)]
        public ParameterCollection InputParameters { get; set; }

        /// <summary>
        /// Gets whether the plug-in is executing from the Microsoft Dynamics 365 for Microsoft Office Outlook with Offline Access client while it is offline.
        /// </summary>
        [DataMember(Order = 6)]
        public bool IsExecutingOffline { get; set; }

        /// <summary>
        /// Gets a value indicating if the plug-in is executing within the database transaction.
        /// </summary>
        [DataMember(Order = 7)]
        public bool IsInTransaction
        {
            get
            {
                return Stage == (int)ProcessingStepStage.Preoperation || Stage == (int)ProcessingStepStage.Postoperation && Mode == (int)ProcessingStepMode.Synchronous;
            }
            set {  /* This property is writable only to correctly support serialization/deserialization */ }
        }

        /// <summary>
        /// Gets a value indicating if the plug-in is executing as a result of the Microsoft Dynamics 365 for Microsoft Office Outlook with Offline Access client transitioning from offline to online and synchronizing with the Microsoft Dynamics 365 server.
        /// </summary>
        [DataMember(Order = 8)]
        public bool IsOfflinePlayback { get; set; }

        /// <summary>
        /// Gets a value indicating if the plug-in is executing in the sandbox.
        /// </summary>
        [DataMember(Order = 9)]
        public int IsolationMode { get; set; }

        /// <summary>
        /// Gets the name of the Web service message that is being processed by the event execution pipeline.
        /// </summary>
        [DataMember(Order = 10)]
        public string MessageName { get; set; }

        /// <summary>
        /// Gets the mode of plug-in execution.
        /// </summary>
        [DataMember(Order = 11)]
        public int Mode { get; set; }

        /// <summary>
        /// Gets the date and time that the related System Job was created.
        /// </summary>
        [DataMember(Order = 12)]
        public DateTime OperationCreatedOn { get; set; }

        /// <summary>
        /// Gets the GUID of the related System Job.
        /// </summary>
        [DataMember(Order = 13)]
        public Guid OperationId { get; set; }

        /// <summary>
        /// Gets the GUID of the organization that the entity belongs to and the plug-in executes under.
        /// </summary>
        [DataMember(Order = 14)]
        public Guid OrganizationId { get; set; }

        /// <summary>
        /// Gets the unique name of the organization that the entity currently being processed belongs to and the plug-in executes under.
        /// </summary>
        [DataMember(Order = 15)]
        public string OrganizationName { get; set; }

        /// <summary>
        /// Gets the parameters of the response message after the core platform operation has completed.
        /// </summary>
        [DataMember(Order = 16)]
        public ParameterCollection OutputParameters { get; set; }

        /// <summary>
        /// Gets a reference to the related SdkMessageProcessingingStep or ServiceEndpoint
        /// </summary>
        [DataMember(Order = 17)]
        public EntityReference OwningExtension { get; set; }

        /// <summary>
        /// Gets the properties of the primary entity after the core platform operation has been completed.
        /// </summary>
        [DataMember(Order = 18)]
        public EntityImageCollection PostEntityImages { get; set; }

        /// <summary>
        /// Gets the properties of the primary entity before the core platform operation has begins.
        /// </summary>
        [DataMember(Order = 19)]
        public EntityImageCollection PreEntityImages { get; set; }

        /// <summary>
        /// Gets the GUID of the primary entity for which the pipeline is processing events.
        /// </summary>
        [DataMember(Order = 20)]
        public Guid PrimaryEntityId { get; set; }

        /// <summary>
        /// Gets the name of the primary entity for which the pipeline is processing events.
        /// </summary>
        [DataMember(Order = 21)]
        public string PrimaryEntityName { get; set; }

        /// <summary>
        /// Gets the GUID of the request being processed by the event execution pipeline.
        /// </summary>
        [DataMember(Order = 22)]
        public Guid? RequestId { get; set; }

        /// <summary>
        /// Gets the name of the secondary entity that has a relationship with the primary entity.
        /// </summary>
        [DataMember(Order = 23)]
        public string SecondaryEntityName { get; set; }

        /// <summary>
        /// Gets the custom properties that are shared between plug-ins.
        /// </summary>
        [DataMember(Order = 24)]
        public ParameterCollection SharedVariables { get; set; }

        /// <summary>
        /// Gets the GUID of the system user for whom the plug-in invokes web service methods on behalf of.
        /// </summary>
        [DataMember(Order = 25)]
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets the execution context from the parent pipeline operation.
        /// </summary>
        [DataMember(Order = 26)]
        public IPluginExecutionContext ParentContext { get; set; }

        /// <summary>
        /// Gets the stage in the execution pipeline that a synchronous plug-in is registered for.
        /// </summary>
        [DataMember(Order = 27)]
        public int Stage { get; set; }

        /// <summary>
        /// Factory method that will return a new fake plugin context instance
        /// </summary>
        /// <returns></returns>
        public static XrmFakedPluginExecutionContext New()
        {
            #if FAKE_XRM_EASY_9
            return new XrmFakedPluginExecutionContext4();
            #else
            return new XrmFakedPluginExecutionContext();
            #endif
        }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public XrmFakedPluginExecutionContext()
        {
            Depth = 1;
            IsExecutingOffline = false;
            MessageName = "Create"; //Default value,
            IsolationMode = 1;
        }

        /// <summary>
        /// Generates a fake plugin execution context from a previously, compressed, plugin execution, which can then be replayed
        /// </summary>
        /// <param name="sSerialisedCompressedProfile"></param>
        /// <returns></returns>
        public static XrmFakedPluginExecutionContext FromSerialisedAndCompressedProfile(string sSerialisedCompressedProfile)
        {
            byte[] data = Convert.FromBase64String(sSerialisedCompressedProfile);

            using (var memStream = new MemoryStream(data))
            {
                using (var decompressedStream = new DeflateStream(memStream, CompressionMode.Decompress, false))
                {
                    byte[] buffer = new byte[0x1000];

                    using (var tempStream = new MemoryStream())
                    {
                        int numBytesRead = decompressedStream.Read(buffer, 0, buffer.Length);
                        while (numBytesRead > 0)
                        {
                            tempStream.Write(buffer, 0, numBytesRead);
                            numBytesRead = decompressedStream.Read(buffer, 0, buffer.Length);
                        }

                        //tempStream has the decompressed plugin context now
                        var decompressedString = Encoding.UTF8.GetString(tempStream.ToArray());
                        var xlDoc = XDocument.Parse(decompressedString);

                        var contextElement = xlDoc.Descendants().Elements()
                            .Where(x => x.Name.LocalName.Equals("Context"))
                            .FirstOrDefault();

                        var pluginContextString = contextElement?.Value;

                        XrmFakedPluginExecutionContext pluginContext = null;
                        using (var reader = new MemoryStream(Encoding.UTF8.GetBytes(pluginContextString)))
                        {
                            var dcSerializer = new DataContractSerializer(typeof(XrmFakedPluginExecutionContext));
                            pluginContext = (XrmFakedPluginExecutionContext)dcSerializer.ReadObject(reader);
                        }

                        return pluginContext;
                    }
                }
            }
        }
    }
}