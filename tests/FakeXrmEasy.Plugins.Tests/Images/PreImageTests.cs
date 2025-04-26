using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Plugins.PluginImages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.ServiceModel.Channels;
using Microsoft.Crm.Sdk.Messages;
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
        [InlineData(MessageNameConstants.Create, false)]
        [InlineData(MessageNameConstants.Update, true)]
        [InlineData(MessageNameConstants.Delete, true)]
        [InlineData(MessageNameConstants.Send, true)]
        [InlineData(MessageNameConstants.Assign, true)]
        public void Should_return_valid_availability(string messageName, bool isAvailable)
        {
            Assert.Equal(isAvailable, PreImage.IsAvailableFor(messageName));
        }
    }
}
