using Crm;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Crud;
using FakeXrmEasy.Middleware.Messages;
using FakeXrmEasy.Middleware.Pipeline;
using FakeXrmEasy.Plugins.Middleware.Pipeline.Exceptions;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.PluginSteps.PluginStepRegistrationFieldNames;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline
{
    public class CustomAutomaticRegistrationTests
    {
        private readonly Assembly _testPluginAssembly;

        public CustomAutomaticRegistrationTests()
        {
            _testPluginAssembly = Assembly.GetAssembly(typeof(FollowupPlugin));
        }

        private IXrmFakedContext CreateContextWithCustomFunction(IEnumerable<Assembly> assemblies, Func<Assembly, IEnumerable<PluginStepDefinition>> func)
        {
            return MiddlewareBuilder
                        .New()

                        // Add* -> Middleware configuration
                        .AddCrud()
                        .AddFakeMessageExecutors()
                        .AddPipelineSimulation(new PipelineOptions()
                        {
                            UseAutomaticPluginStepRegistration = true,
                            PluginAssemblies = assemblies,
                            CustomPluginStepDiscoveryFunction = func
                        })

                        // Use* -> Defines pipeline sequence
                        .UsePipelineSimulation()
                        .UseCrud()
                        .UseMessages()

                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();
        }

        [Fact]
        public void Should_throw_exception_with_automatic_registration_if_custom_func_is_null()
        {
            Assert.Throws<CustomDiscoveryFunctionMissingException>(() => CreateContextWithCustomFunction(new List<Assembly>()
            {
                _testPluginAssembly
            }, null));
        }

        [Fact]
        public void Should_not_register_if_custom_func_returned_empty()
        {
            var context = CreateContextWithCustomFunction(
                new List<Assembly>()
                {
                    _testPluginAssembly
                },
                (Assembly assembly) => new List<PluginStepDefinition>()
           );

            var followupPluginType = context.CreateQuery(PluginStepRegistrationEntityNames.PluginType)
                            .FirstOrDefault();

            Assert.Null(followupPluginType);
        }

        [Fact]
        public void Should_throw_exception_when_registering_a_plugin_step_without_plugin_type()
        {
            Assert.Throws<MissingPluginTypeInPluginStepDefinitionException>(() => CreateContextWithCustomFunction(
                new List<Assembly>()
                {
                    _testPluginAssembly
                },
                (Assembly assembly) => new List<PluginStepDefinition>() {
                    new PluginStepDefinition()
                    {
                        PluginType = "",
                        EntityLogicalName = Contact.EntityLogicalName,
                        Stage = ProcessingStepStage.Preoperation,
                        Mode = ProcessingStepMode.Synchronous,
                        MessageName = "Update",
                        Rank = 2
                    }
                }
           ));
        }

        [Fact]
        public void Should_register_plugin_step_with_custom_func_and_exact_same_properties()
        {
            var context = CreateContextWithCustomFunction(
                new List<Assembly>()
                {
                    _testPluginAssembly
                },
                (Assembly assembly) => new List<PluginStepDefinition>() {
                    new PluginStepDefinition()
                    {
                        PluginType = typeof(FollowupPlugin).FullName,
                        EntityLogicalName = Contact.EntityLogicalName,
                        Stage = ProcessingStepStage.Preoperation,
                        Mode = ProcessingStepMode.Synchronous,
                        MessageName = "Update",
                        Rank = 2
                    }
                }
           );

            var step = context.CreateQuery<SdkMessageProcessingStep>()
                            .FirstOrDefault();

            var pluginType = context.CreateQuery(PluginStepRegistrationEntityNames.PluginType).FirstOrDefault();
            var sdkMessage = context.CreateQuery<SdkMessage>().FirstOrDefault();
            var sdkMessageFilter = context.CreateQuery<SdkMessageFilter>().FirstOrDefault();

            Assert.NotNull(pluginType);
            Assert.NotNull(step);
            Assert.NotNull(sdkMessage);
            Assert.NotNull(sdkMessageFilter);

            Assert.Equal(typeof(FollowupPlugin).FullName, pluginType[PluginTypeFieldNames.TypeName]);

            Assert.Equal(Contact.EntityLogicalName, sdkMessageFilter[SdkMessageFilterFieldNames.EntityLogicalName]);
            Assert.Equal("Update", sdkMessage.Name);

            Assert.Equal(2, step.Rank);
            Assert.Equal((int)ProcessingStepStage.Preoperation, step.Stage.Value);
            Assert.Equal((int)ProcessingStepMode.Synchronous, step.Mode.Value);

            Assert.Equal(pluginType.Id, ((EntityReference)step["eventhandler"]).Id);
            Assert.Equal(sdkMessage.Id, ((EntityReference)step["sdkmessageid"]).Id);
            Assert.Equal(sdkMessageFilter.Id, ((EntityReference)step["sdkmessagefilterid"]).Id);
        }

        // This test fails - because the plugin is invoked through pipeline simulation, not directly
        [Fact]
        public void When_A_Resource_Request_Is_Created_Without_A_Team_An_Exception_Will_Get_Thrown()
        {
            var context = CreateContextWithCustomFunction(
             new List<Assembly>()
             {
                    _testPluginAssembly
             },
             (Assembly assembly) => new List<PluginStepDefinition>() {
                    new PluginStepDefinition()
                    {
                        PluginType = typeof(ExceptionLoverPlugin).FullName,
                        EntityLogicalName = Contact.EntityLogicalName,
                        Stage = ProcessingStepStage.Preoperation,
                        Mode = ProcessingStepMode.Synchronous,
                        MessageName = "Create",
                        Rank = 2
                    }
             }
        );



            // Arrange
            Contact contact = new Contact() { Id = Guid.NewGuid() };
            // Act
            Action act = () => context.GetOrganizationService().Create(contact);
            //assert
            InvalidPluginExecutionException exception = Assert.Throws<InvalidPluginExecutionException>(act);
            Assert.Equal(ExceptionLoverPlugin.PluginExceptionMessage, exception.Message);
        }
    }
}
