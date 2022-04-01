using FakeXrmEasy.Abstractions.Plugins.Enums;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FakeXrmEasy.Plugins.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c124cb50761165a765adf6078bde555a7c5a2b692ed6e6ec9df0bd7d20da69170bae9bf95e874fa50995cc080af404ccad36515fa509c4ea6599a0502c1642db254a293e023c47c79ce69889c6ba921d124d896d87f0baaa9ea1d87b28589ffbe7b08492606bacef19dc4bc4cefb0d525be63ee722b02dc8c79688a7a8f623a2")]
[assembly: InternalsVisibleTo("FakeXrmEasy.Plugins.Performance, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c124cb50761165a765adf6078bde555a7c5a2b692ed6e6ec9df0bd7d20da69170bae9bf95e874fa50995cc080af404ccad36515fa509c4ea6599a0502c1642db254a293e023c47c79ce69889c6ba921d124d896d87f0baaa9ea1d87b28589ffbe7b08492606bacef19dc4bc4cefb0d525be63ee722b02dc8c79688a7a8f623a2")]

namespace FakeXrmEasy.Plugins.PluginImages
{
    /// <summary>
    /// Defines if PreImage is available for specific requests and stages
    /// 
    /// ---- Message------Stage--------Pre-Image-----Post-Image------
    ///      Create       PRE             No             No
    ///      Update       PRE             Yes            No
    ///      Delete       PRE             Yes            No
    ///      Upsert       PRE             ??             ??
    ///      Create       POST            No             Yes
    ///      Update       POST            Yes            Yes
    ///      Delete       POST            Yes            No
    ///      Upsert       POST            ??             ??
    /// </summary>
    internal class PreImage
    {
        /// <summary>
        /// Determines if a PreImage is available for the given request and plugin stage
        /// </summary>
        /// <param name="organizationRequestType">The request to check the availability for</param>
        /// <param name="stage">The plugin stage to check such availability</param>
        /// <returns></returns>
        internal static bool IsAvailableFor(Type organizationRequestType, ProcessingStepStage stage)
        {
            return organizationRequestType == typeof(UpdateRequest) && stage == ProcessingStepStage.Preoperation 
                || organizationRequestType == typeof(DeleteRequest) && stage == ProcessingStepStage.Preoperation
                || organizationRequestType == typeof(UpdateRequest) && stage == ProcessingStepStage.Postoperation
                || organizationRequestType == typeof(DeleteRequest) && stage == ProcessingStepStage.Postoperation;
        }
    }
}
