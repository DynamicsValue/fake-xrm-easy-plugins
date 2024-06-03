using Crm;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Plugins.Definitions;
using FakeXrmEasy.Plugins.PluginImages;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions;
using System.Collections.Generic;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.PluginSteps
{
    public class PluginStepImagesValidatorTests
    {
        private readonly PluginStepValidator _validator;

        public PluginStepImagesValidatorTests()
        {
            _validator = new PluginStepValidator();
        }

        [Theory]
        [InlineData(MessageNameConstants.Update, ProcessingStepStage.Preoperation)]
        [InlineData(MessageNameConstants.Delete, ProcessingStepStage.Preoperation)]
        [InlineData(MessageNameConstants.Update, ProcessingStepStage.Postoperation)]
        [InlineData(MessageNameConstants.Delete, ProcessingStepStage.Postoperation)]
        public void Should_return_valid_registration_for_valid_preimages(string messageName, ProcessingStepStage stage)
        {
            var pluginStepDefinition = new PluginStepDefinition()
            {
                MessageName = messageName,
                EntityLogicalName = Account.EntityLogicalName,
                Stage = stage,
                ImagesDefinitions = new List<IPluginImageDefinition>()
                {
                    new PluginImageDefinition("PreImage", ProcessingStepImageType.PreImage, new string[] { "name" })
                }
            };
            Assert.True(_validator.IsValid(pluginStepDefinition));
        }

        [Theory]
        [InlineData(MessageNameConstants.Create, ProcessingStepStage.Prevalidation)]
        [InlineData(MessageNameConstants.Create, ProcessingStepStage.Preoperation)]
        public void Should_return_not_valid_registration_for_valid_preimages(string messageName, ProcessingStepStage stage)
        {
            var pluginStepDefinition = new PluginStepDefinition()
            {
                MessageName = messageName,
                EntityLogicalName = Account.EntityLogicalName,
                Stage = stage,
                ImagesDefinitions = new List<IPluginImageDefinition>()
                {
                    new PluginImageDefinition("PreImage", ProcessingStepImageType.PreImage, new string[] { "name" })
                }
            };
            Assert.Throws<PreImageNotAvailableException>(() => _validator.IsValid(pluginStepDefinition));
        }

        [Theory]
        [InlineData(MessageNameConstants.Create, ProcessingStepStage.Postoperation)]
        [InlineData(MessageNameConstants.Update, ProcessingStepStage.Postoperation)]
        public void Should_return_valid_registration_for_valid_postimages(string messageName, ProcessingStepStage stage)
        {
            var pluginStepDefinition = new PluginStepDefinition()
            {
                MessageName = messageName,
                EntityLogicalName = Account.EntityLogicalName,
                Stage = stage,
                ImagesDefinitions = new List<IPluginImageDefinition>()
                {
                    new PluginImageDefinition("PostImage", ProcessingStepImageType.PostImage, new string[] { "name" })
                }
            };
            Assert.True(_validator.IsValid(pluginStepDefinition));
        }

        [Theory]
        [InlineData(MessageNameConstants.Create, ProcessingStepStage.Prevalidation)]
        [InlineData(MessageNameConstants.Update, ProcessingStepStage.Prevalidation)]
        [InlineData(MessageNameConstants.Create, ProcessingStepStage.Preoperation)]
        [InlineData(MessageNameConstants.Update, ProcessingStepStage.Preoperation)]
        public void Should_return_not_valid_registration_for_not_valid_postimages(string messageName, ProcessingStepStage stage)
        {
            var pluginStepDefinition = new PluginStepDefinition()
            {
                MessageName = messageName,
                EntityLogicalName = Account.EntityLogicalName,
                Stage = stage,
                ImagesDefinitions = new List<IPluginImageDefinition>()
                {
                    new PluginImageDefinition("PostImage", ProcessingStepImageType.PostImage, new string[] { "name" })
                }
            };
            Assert.Throws<PostImageNotAvailableException>(() => _validator.IsValid(pluginStepDefinition));
        }
    }
}
