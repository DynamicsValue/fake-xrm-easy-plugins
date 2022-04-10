using Crm;
using FakeItEasy;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Pipeline;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions;
using FakeXrmEasy.Tests.PluginsForTesting;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.PluginSteps
{
    public class PluginRegistrationValidationTests
    {
        private IXrmFakedContext _context;
        private readonly IPluginStepValidator _invalidValidator;
        private readonly IPluginStepValidator _validValidator;

        public PluginRegistrationValidationTests()
        {
            _validValidator = A.Fake<IPluginStepValidator>();
            _invalidValidator = A.Fake<IPluginStepValidator>();

            A.CallTo(() => _invalidValidator.IsValid(A<string>.Ignored, A<string>.Ignored, A<ProcessingStepStage>.Ignored, A<ProcessingStepMode>.Ignored)).ReturnsLazily(() => false);
            A.CallTo(() => _validValidator.IsValid(A<string>.Ignored, A<string>.Ignored, A<ProcessingStepStage>.Ignored, A<ProcessingStepMode>.Ignored)).ReturnsLazily(() => true);
        }

        [Fact]
        public void Should_return_error_when_registering_an_invalid_plugin_step_against_any_entity_if_validation_is_enabled()
        {
            _context = MiddlewareBuilder
                        .New()
                        .AddPipelineSimulation(new PipelineOptions() { UsePluginStepRegistrationValidation = true })
                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();

            _context.SetProperty(_invalidValidator);
            Assert.Throws<InvalidPluginStepRegistrationException>(() => _context.RegisterPluginStep<AccountNumberPlugin>(MessageNameConstants.Create, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous));
            A.CallTo(() => _invalidValidator.IsValid(MessageNameConstants.Create, "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)).MustHaveHappened();
        }

        [Fact]
        public void Should_return_error_when_registering_an_invalid_plugin_step_against_specific_entity_if_validation_is_enabled()
        {
            _context = MiddlewareBuilder
                        .New()
                        .AddPipelineSimulation(new PipelineOptions() { UsePluginStepRegistrationValidation = true })
                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();

            _context.SetProperty(_invalidValidator);
            Assert.Throws<InvalidPluginStepRegistrationException> (() => _context.RegisterPluginStep<AccountNumberPlugin, Account>(MessageNameConstants.Create, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous));
            
            A.CallTo(() => _invalidValidator.IsValid(MessageNameConstants.Create, EntityLogicalNameContants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)).MustHaveHappened();
            A.CallTo(() => _invalidValidator.IsValid(MessageNameConstants.Create, "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)).MustNotHaveHappened();
        }

        [Fact]
        public void Should_not_return_error_when_registering_an_valid_plugin_step_against_any_entity_if_validation_is_enabled()
        {
            _context = MiddlewareBuilder
                        .New()
                        .AddPipelineSimulation(new PipelineOptions() { UsePluginStepRegistrationValidation = true })
                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();

            _context.SetProperty(_validValidator);
            _context.RegisterPluginStep<AccountNumberPlugin>(MessageNameConstants.Create, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous);

            A.CallTo(() => _validValidator.IsValid(MessageNameConstants.Create, "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)).MustHaveHappened();
        }

        [Fact]
        public void Should_not_return_error_when_registering_an_valid_plugin_step_against_specific_entity_if_validation_is_enabled()
        {
            _context = MiddlewareBuilder
                        .New()
                        .AddPipelineSimulation(new PipelineOptions() { UsePluginStepRegistrationValidation = true })
                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();

            _context.SetProperty(_validValidator);
            _context.RegisterPluginStep<AccountNumberPlugin, Account>(MessageNameConstants.Create, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous);

            A.CallTo(() => _validValidator.IsValid(MessageNameConstants.Create, EntityLogicalNameContants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)).MustHaveHappened();
            A.CallTo(() => _validValidator.IsValid(MessageNameConstants.Create, "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)).MustNotHaveHappened();
        }

        [Fact]
        public void Should_not_return_error_when_registering_an_invalid_plugin_step_if_validation_is_disabled()
        {
            _context = MiddlewareBuilder
                        .New()
                        .AddPipelineSimulation(new PipelineOptions() { UsePluginStepRegistrationValidation = false })
                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();

            _context.SetProperty(_invalidValidator);
            _context.RegisterPluginStep<AccountNumberPlugin>(MessageNameConstants.Create, ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous);

            A.CallTo(() => _invalidValidator.IsValid(MessageNameConstants.Create, "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)).MustNotHaveHappened();
        }
    }
}
