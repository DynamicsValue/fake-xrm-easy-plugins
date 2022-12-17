using FakeXrmEasy.Abstractions.Enums.CustomApis;
using FakeXrmEasy.Plugins.Middleware.CustomApis;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Middleware.CustomApis
{
    public class CustomApiFakeMessageExecutorTests : FakeXrmEasyTestsBase
    {
        private class MyFakePlugin : IPlugin
        {
            public void Execute(IServiceProvider serviceProvider)
            {
                var pluginContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                pluginContext.OutputParameters.Add(new KeyValuePair<string, object>("DummyKey", "DummyValue"));
            }
        }

        private class MyCustomApiRequest: OrganizationRequest
        {
            public static string MessageName = "FakeRequest";
            public MyCustomApiRequest()
            {
                RequestName = MessageName;
            }
        }

        private class MyGenericCustomApiWithPluginExecutor: CustomApiFakeMessageExecutor<MyFakePlugin>
        {
            public override string MessageName => MyCustomApiRequest.MessageName;
        }

        [Fact]
        public void Should_set_plugin_type_and_message_name_properties_for_early_bound_custom_apis()
        {
            var customApi = new CustomApiFakeMessageExecutor<MyFakePlugin, MyCustomApiRequest>();

            Assert.NotNull(customApi.PluginType);

            Assert.IsType<MyFakePlugin>(customApi.PluginType);
            Assert.Equal(MyCustomApiRequest.MessageName, customApi.MessageName);

            Assert.Equal(CustomProcessingStepType.None, customApi.CustomProcessingType);
            Assert.Equal(BindingType.Global, customApi.BindingType);
            Assert.Null(customApi.EntityLogicalName);

            Assert.Equal(typeof(MyCustomApiRequest), customApi.GetResponsibleRequestType());

            Assert.True(customApi.CanExecute(new MyCustomApiRequest()));
            Assert.False(customApi.CanExecute(new OrganizationRequest()));
        }

        [Fact]
        public void Should_execute_plugin_for_early_bound_custom_apis()
        {
            var customApi = new CustomApiFakeMessageExecutor<MyFakePlugin, MyCustomApiRequest>();
            var response = customApi.Execute(new MyCustomApiRequest(), _context);
            var outputParameters = response.Results;
            Assert.NotEmpty(outputParameters);

            var responseValue = outputParameters["DummyKey"] as string;
            Assert.Equal("DummyValue", responseValue);
        }

        [Fact]
        public void Should_set_message_name_properties_for_generic_custom_apis()
        {
            var customApi = new MyGenericCustomApiWithPluginExecutor();

            Assert.NotNull(customApi.PluginType);

            Assert.IsType<MyFakePlugin>(customApi.PluginType);
            Assert.Equal(MyCustomApiRequest.MessageName, customApi.MessageName);

            Assert.Equal(CustomProcessingStepType.None, customApi.CustomProcessingType);
            Assert.Equal(BindingType.Global, customApi.BindingType);
            Assert.Null(customApi.EntityLogicalName);

            var type = customApi.GetResponsibleRequestType();
            Assert.Equal(typeof(OrganizationRequest), type);

            Assert.True(customApi.CanExecute(new OrganizationRequest() { RequestName = MyCustomApiRequest.MessageName }));
            Assert.False(customApi.CanExecute(new OrganizationRequest() { RequestName = "Other" }));
        }

        [Fact]
        public void Should_execute_plugin_for_generic_custom_apis()
        {
            var customApi = new MyGenericCustomApiWithPluginExecutor();
            var response = customApi.Execute(new OrganizationRequest() { RequestName = MyCustomApiRequest.MessageName }, _context);
            var outputParameters = response.Results;
            Assert.NotEmpty(outputParameters);

            var responseValue = outputParameters["DummyKey"] as string;
            Assert.Equal("DummyValue", responseValue);
        }

        [Fact]
        public void Should_throw_exception_when_using_default_implementation_without_setting_a_message_name()
        {
            var customApi = new CustomApiFakeMessageExecutor<MyFakePlugin>();

            Assert.NotNull(customApi.PluginType);

            Assert.IsType<MyFakePlugin>(customApi.PluginType);
            Assert.Throws<NotImplementedException>(() => customApi.MessageName);
        }
    }
}
