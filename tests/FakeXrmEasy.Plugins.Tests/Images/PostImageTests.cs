using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Plugins.PluginImages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using Microsoft.Crm.Sdk.Messages;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Images
{
    public class PostImageTests
    {
        /// Defines if PostImage is available for specific requests and stages
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
        ///      
        [Theory]
        [InlineData(MessageNameConstants.Create, ProcessingStepStage.Postoperation, true)]
        [InlineData(MessageNameConstants.Update, ProcessingStepStage.Postoperation, true)]
        [InlineData(MessageNameConstants.Delete, ProcessingStepStage.Postoperation, false)]
        [InlineData(MessageNameConstants.Send, ProcessingStepStage.Postoperation, true)]
        public void Should_return_valid_availability(string messageName, ProcessingStepStage stage, bool isAvailable)
        {
            Assert.Equal(isAvailable, PostImage.IsAvailableFor(messageName, stage));
        }
    }
}
