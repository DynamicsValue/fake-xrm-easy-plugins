using Crm;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.PluginImages;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Linq;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline.RegisteredPluginStepsRetrieverTests
{
    public class GetPluginImageDefinitionsWithRetrieveMultipleTests : FakeXrmEasyTestsBase
    {
        [Fact]
        public void Should_return_empty_plugin_image_list_if_none_were_registered()
        {
            var pluginImages = RegisteredPluginStepsRetriever.GetPluginImageDefinitions(_context,Guid.NewGuid(), ProcessingStepImageType.Both);
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
        public void Should_filter_image_by_relevant_pluging_step_and_not_others(ProcessingStepImageType registrationType, ProcessingStepImageType queryType, bool shouldContainImage)
        {
            string registeredPreImageName = "PreImage";
            PluginImageDefinition preImageDefinition = new PluginImageDefinition(registeredPreImageName, registrationType);

            var pluginStepIdWithImages = _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = "Create",
                Stage = ProcessingStepStage.Preoperation,
                Mode = ProcessingStepMode.Synchronous,
                ImagesDefinitions = new PluginImageDefinition[] { preImageDefinition }
            });
            
            var otherPluginStepId = _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = "Create",
                Stage = ProcessingStepStage.Preoperation,
                Mode = ProcessingStepMode.Synchronous
            });

            var pluginImagesFirstPluginStep = RegisteredPluginStepsRetriever.GetPluginImageDefinitions(_context, pluginStepIdWithImages, queryType).ToList();
            var pluginImagesSecondPluginStep = RegisteredPluginStepsRetriever.GetPluginImageDefinitions(_context, otherPluginStepId, queryType).ToList();

            Assert.Equal(shouldContainImage, pluginImagesFirstPluginStep.Count > 0);
            Assert.Empty(pluginImagesSecondPluginStep);

            if(shouldContainImage)
            {
                Assert.True(pluginImagesFirstPluginStep[0].Attributes.Contains("name"));
                Assert.True(pluginImagesFirstPluginStep[0].Attributes.Contains("imagetype"));
            }

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
        public void Should_filter_image_by_relevant_pluging_step_and_not_others_with_attributes(ProcessingStepImageType registrationType, ProcessingStepImageType queryType, bool shouldContainImage)
        {
            string registeredPreImageName = "PreImage";
            PluginImageDefinition preImageDefinition = new PluginImageDefinition(registeredPreImageName, registrationType, new string[] { "name" });

            var pluginStepIdWithImages = _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = "Create",
                Stage = ProcessingStepStage.Preoperation,
                Mode = ProcessingStepMode.Synchronous,
                ImagesDefinitions = new PluginImageDefinition[] { preImageDefinition }
            });

            var otherPluginStepId = _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = "Create",
                Stage = ProcessingStepStage.Preoperation,
                Mode = ProcessingStepMode.Synchronous
            });

            var pluginImagesFirstPluginStep = RegisteredPluginStepsRetriever.GetPluginImageDefinitions(_context, pluginStepIdWithImages, queryType).ToList();
            var pluginImagesSecondPluginStep = RegisteredPluginStepsRetriever.GetPluginImageDefinitions(_context, otherPluginStepId, queryType).ToList();

            Assert.Equal(shouldContainImage, pluginImagesFirstPluginStep.Count > 0);
            Assert.Empty(pluginImagesSecondPluginStep);

            if (shouldContainImage)
            {
                Assert.True(pluginImagesFirstPluginStep[0].Attributes.Contains("attributes"));
            }

        }
    }
}
