using FakeXrmEasy.Plugins.Middleware.CustomApis;
using Microsoft.Xrm.Sdk;
using System;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Middleware.CustomApis
{
    public class CustomApiFakeMessageExecutorTests
    {
        private class MyFakePlugin : IPlugin
        {
            public void Execute(IServiceProvider serviceProvider)
            {
                //Do nothing...
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

            Assert.Equal(typeof(MyCustomApiRequest), customApi.GetResponsibleRequestType());

            Assert.True(customApi.CanExecute(new MyCustomApiRequest()));
            Assert.False(customApi.CanExecute(new OrganizationRequest()));
        }

        [Fact]
        public void Should_set_message_name_properties_for_generic_custom_apis()
        {
            var customApi = new MyGenericCustomApiWithPluginExecutor();

            Assert.NotNull(customApi.PluginType);

            Assert.IsType<MyFakePlugin>(customApi.PluginType);
            Assert.Equal(MyCustomApiRequest.MessageName, customApi.MessageName);

            var type = customApi.GetResponsibleRequestType();
            Assert.Equal(typeof(OrganizationRequest), type);

            Assert.True(customApi.CanExecute(new OrganizationRequest() { RequestName = MyCustomApiRequest.MessageName }));
            Assert.False(customApi.CanExecute(new OrganizationRequest() { RequestName = "Other" }));
        }
    }
}
