using Crm;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Pipeline;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.PluginSteps.PluginStepRegistrationFieldNames;
using FakeXrmEasy.Tests.PluginsForTesting;
using System.Linq;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.PluginSteps
{
    /// <summary>
    /// Unit tests to check the actual plugin is effectively registered in the In-Memory db
    /// </summary>
    public class PluginStepBasicRegistrationTests
    {
        private IXrmFakedContext _context;

        public PluginStepBasicRegistrationTests()
        {
            _context = MiddlewareBuilder
                        .New()
                        .AddPipelineSimulation(new PipelineOptions() { UsePipelineSimulation = true })
                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();
        }

        [Theory]
        [InlineData(MessageNameConstants.Create, EntityLogicalNameContants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameContants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Delete, EntityLogicalNameContants.Account, ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Retrieve, EntityLogicalNameContants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameContants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(MessageNameConstants.Update, EntityLogicalNameContants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_register_plugin_step_with_plugin_definition_and_plugin_type_signature(string messageName, 
                                                        string entityLogicalName, 
                                                        ProcessingStepStage stage, 
                                                        ProcessingStepMode mode)
        {
            var pluginStepId = _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = messageName,
                EntityLogicalName = entityLogicalName,
                Stage = stage,
                Mode = mode
            });

            var processingStep = _context.CreateQuery<SdkMessageProcessingStep>().FirstOrDefault();
            Assert.NotNull(processingStep);
            Assert.Equal(pluginStepId, processingStep.Id);
            Assert.Equal((int)stage, processingStep.Stage.Value);
            Assert.Equal((int)mode, processingStep.Mode.Value);

            var sdkMessage = _context.CreateQuery<SdkMessage>()
                                    .Where(mes => mes.Id == processingStep.SdkMessageId.Id)
                                    .FirstOrDefault();
            Assert.NotNull(sdkMessage);
            Assert.Equal(messageName, sdkMessage.Name);

            var sdkMessageFilter = _context.CreateQuery<SdkMessageFilter>()
                                    .Where(messageFilter => messageFilter.Id == processingStep.SdkMessageFilterId.Id)
                                    .FirstOrDefault();
            Assert.NotNull(sdkMessageFilter);
            Assert.Equal(entityLogicalName, (string) sdkMessageFilter[SdkMessageFilterFieldNames.EntityLogicalName]);
        }
    }
}
