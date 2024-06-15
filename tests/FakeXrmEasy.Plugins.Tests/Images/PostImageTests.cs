using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Plugins.PluginImages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
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
        [InlineData(typeof(OrganizationRequest), ProcessingStepStage.MainOperation, false)]
        [InlineData(typeof(CreateRequest), ProcessingStepStage.Postoperation, true)]
        [InlineData(typeof(UpdateRequest), ProcessingStepStage.Postoperation, true)]
        [InlineData(typeof(DeleteRequest), ProcessingStepStage.Postoperation, false)]
        public void Should_return_valid_availability(Type orgRequestType, ProcessingStepStage stage, bool isAvailable)
        {
            Assert.Equal(isAvailable, PostImage.IsAvailableFor(orgRequestType, stage));
        }
    }
}
