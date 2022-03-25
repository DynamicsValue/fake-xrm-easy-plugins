using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Plugins.Images;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Images
{
    public class PreImageTests
    {
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
        ///      
        [Theory]
        [InlineData(typeof(OrganizationRequest), ProcessingStepStage.MainOperation, false)]
        [InlineData(typeof(CreateRequest), ProcessingStepStage.Preoperation, false)]
        [InlineData(typeof(UpdateRequest), ProcessingStepStage.Preoperation, true)]
        [InlineData(typeof(DeleteRequest), ProcessingStepStage.Preoperation, true)]
        [InlineData(typeof(CreateRequest), ProcessingStepStage.Postoperation, false)]
        [InlineData(typeof(UpdateRequest), ProcessingStepStage.Postoperation, true)]
        [InlineData(typeof(DeleteRequest), ProcessingStepStage.Postoperation, true)]
        public void Should_return_valid_availability(Type orgRequestType, ProcessingStepStage stage, bool isAvailable)
        {
            Assert.Equal(isAvailable, PreImage.IsAvailableFor(orgRequestType, stage));
        }
    }
}
