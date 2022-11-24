using Crm;
using FakeItEasy;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Pipeline;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.Definitions;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.PluginSteps.InvalidRegistrationExceptions;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.PluginSteps
{
    /// <summary>
    /// Makes sure to run plugin step registration validation if enabled in pipeline
    /// </summary>
    public class PluginRegistrationValidationTests
    {
        private IXrmFakedContext _context;
        private readonly IPluginStepValidator _invalidValidator;
        private readonly IPluginStepValidator _validValidator;

        public PluginRegistrationValidationTests()
        {
            _validValidator = A.Fake<IPluginStepValidator>();
            _invalidValidator = A.Fake<IPluginStepValidator>();

            A.CallTo(() => _invalidValidator.IsValid(A<IPluginStepDefinition>.Ignored)).ReturnsLazily(() => false);
            A.CallTo(() => _validValidator.IsValid(A<IPluginStepDefinition>.Ignored)).ReturnsLazily(() => true);
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
            Assert.Throws<InvalidPluginStepRegistrationException>(() =>
                _context.RegisterPluginStep<FollowupPlugin2>(new PluginStepDefinition()
                {
                    MessageName = "Create",
                    Stage = ProcessingStepStage.Preoperation
                })
            );

            A.CallTo(() => _invalidValidator.IsValid(A<IPluginStepDefinition>.Ignored)).MustHaveHappened();
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
            Assert.Throws<InvalidPluginStepRegistrationException> (() =>
                _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
                {
                    MessageName = "Create",
                    EntityLogicalName = Account.EntityLogicalName,
                    Stage = ProcessingStepStage.Preoperation
                })
            );
            
            //A.CallTo(() => _invalidValidator.IsValid(MessageNameConstants.Create, EntityLogicalNameContants.Account, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)).MustHaveHappened();
            //A.CallTo(() => _invalidValidator.IsValid(MessageNameConstants.Create, "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)).MustNotHaveHappened();
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
            _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = MessageNameConstants.Create,
                Stage = ProcessingStepStage.Postoperation,
                Mode = ProcessingStepMode.Synchronous
            });

            //A.CallTo(() => _validValidator.IsValid(MessageNameConstants.Create, "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)).MustHaveHappened();
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
            _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = MessageNameConstants.Create,
                EntityLogicalName = Account.EntityLogicalName,
                Stage = ProcessingStepStage.Postoperation,
                Mode = ProcessingStepMode.Synchronous
            });

            //A.CallTo(() => _validValidator.IsValid(MessageNameConstants.Create, EntityLogicalNameContants.Account, ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)).MustHaveHappened();
            //A.CallTo(() => _validValidator.IsValid(MessageNameConstants.Create, "*", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)).MustNotHaveHappened();
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
            _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
            {
                MessageName = MessageNameConstants.Create,
                Stage = ProcessingStepStage.Preoperation,
                Mode = ProcessingStepMode.Asynchronous
            });

            //A.CallTo(() => _invalidValidator.IsValid(MessageNameConstants.Create, "*", ProcessingStepStage.Preoperation, ProcessingStepMode.Asynchronous)).MustNotHaveHappened();
        }


        /// <summary>
        /// We're keeping this one with an obsolete method cause the test was only applicable to such obsolete method
        /// </summary>
        [Fact]
        public void Should_return_error_when_registering_plugin_step_with_early_bound_and_no_entity_type_code()
        {
            _context = MiddlewareBuilder
                        .New()
                        .AddPipelineSimulation(new PipelineOptions() { UsePluginStepRegistrationValidation = true })
                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();

            _context.SetProperty(_validValidator);
            Assert.Throws<EntityTypeCodeNotFoundException>(() =>
                _context.RegisterPluginStep<AccountNumberPlugin, EarlyBoundEntityWithNoEntityTypeCode>(MessageNameConstants.Create, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)
            );
        }

        [Fact]
        public void Should_return_error_when_registering_plugin_step_with_early_bound_method_and_late_bound_entity()
        {
            _context = MiddlewareBuilder
                        .New()
                        .AddPipelineSimulation(new PipelineOptions() { UsePluginStepRegistrationValidation = true })
                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();

            _context.SetProperty(_validValidator);
            Assert.Throws<InvalidRegistrationMethodForLateBoundException>(() => 
                _context.RegisterPluginStep<AccountNumberPlugin, Entity>(MessageNameConstants.Create, ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)
            );
        }
    }
}
