using Crm;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.PluginImages;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Linq;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.IXrmFakedContextPipelineExtensions
{
    public class GetPluginImageDefinitionsWithRetrieveMultipleTests : FakeXrmEasyTestsBase
    {
        [Fact]
        public void Should_return_empty_plugin_image_list_if_none_were_registered()
        {
            var pluginImages = _context.GetPluginImageDefinitions(Guid.NewGuid(), ProcessingStepImageType.Both);
            Assert.Empty(pluginImages);
        }

        [Theory]
        [InlineData(ProcessingStepImageType.PreImage, ProcessingStepImageType.PreImage, true)]
        [InlineData(ProcessingStepImageType.PostImage, ProcessingStepImageType.PostImage, true)]
        [InlineData(ProcessingStepImageType.Both, ProcessingStepImageType.PreImage, true)]
        [InlineData(ProcessingStepImageType.Both, ProcessingStepImageType.PostImage, true)]
        [InlineData(ProcessingStepImageType.PreImage, ProcessingStepImageType.PostImage, false)]
        [InlineData(ProcessingStepImageType.PostImage, ProcessingStepImageType.PreImage, false)]
        [InlineData(ProcessingStepImageType.PreImage, ProcessingStepImageType.Both, false)]
        [InlineData(ProcessingStepImageType.PostImage, ProcessingStepImageType.Both, false)]
        public void Should_filter_image_by_relevant_pluging_step_and_not_others(ProcessingStepImageType registrationType, ProcessingStepImageType queryType, bool shouldContain)
        {
            string registeredPreImageName = "PreImage";
            PluginImageDefinition preImageDefinition = new PluginImageDefinition(registeredPreImageName, registrationType);

            var pluginStepIdWithImages = _context.RegisterPluginStep<AccountNumberPlugin>("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous, registeredImages: new PluginImageDefinition[] { preImageDefinition });
            var otherPluginStepId = _context.RegisterPluginStep<AccountNumberPlugin>("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous);

            var pluginImagesFirstPluginStep = _context.GetPluginImageDefinitions(pluginStepIdWithImages, queryType).ToList();
            var pluginImagesSecondPluginStep = _context.GetPluginImageDefinitions(otherPluginStepId, queryType).ToList();

            Assert.Equal(shouldContain, pluginImagesFirstPluginStep.Count > 0);
            Assert.Empty(pluginImagesSecondPluginStep);
        }
    }
}
