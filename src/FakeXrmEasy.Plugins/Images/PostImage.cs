using FakeXrmEasy.Abstractions.Plugins.Enums;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;

namespace FakeXrmEasy.Plugins.Images
{
    /// <summary>
    /// Defines a PostImage is available for specific requests and stages
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
    internal class PostImage
    {
        /// <summary>
        /// Determines if a PostImage is available for the given request and plugin stage
        /// </summary>
        /// <param name="organizationRequestType">The request to check the availability for</param>
        /// <param name="stage">The plugin stage to check such availability</param>
        /// <returns></returns>
        internal static bool IsAvailableFor(Type organizationRequestType, ProcessingStepStage stage)
        {
            return organizationRequestType == typeof(CreateRequest) && stage == ProcessingStepStage.Postoperation
                || organizationRequestType == typeof(UpdateRequest) && stage == ProcessingStepStage.Postoperation;
        }
    }
}
