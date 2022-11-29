using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Crm;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Crud;
using FakeXrmEasy.Middleware.Messages;
using FakeXrmEasy.Middleware.Pipeline;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.Audit;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Issues
{
    public class Issue50 
    {
        private readonly IXrmFakedContext _context;
        private readonly IOrganizationService _service;

        public Issue50()
        {
            _context = MiddlewareBuilder
                .New()

                // Add* -> Middleware configuration
                .AddCrud()
                .AddFakeMessageExecutors()
                .AddGenericFakeMessageExecutors(Assembly.GetAssembly(typeof(CustomApiRequestExecutor)))
                .AddPipelineSimulation(new PipelineOptions() { UsePluginStepAudit = true })

                // Use* -> Defines pipeline sequence
                .UsePipelineSimulation()
                .UseCrud()
                .UseMessages()

                .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                .Build();

            _service = _context.GetOrganizationService();
        }

        [Fact]
        public void api_plugin_should_work_standalone_if_called_directly()
        {
            // arrange
            var initialName = "Initial Name";
            var expectedName = initialName;

            var pluginCtx = _context.GetDefaultPluginContext();
            pluginCtx.MessageName = FakeApiPlugin.Message;
            pluginCtx.Stage = 30;
            pluginCtx.InputParameters = new ParameterCollection
            {
                ["name"] = initialName
            };
            pluginCtx.OutputParameters = new ParameterCollection();

            // act

            _context.ExecutePluginWith<FakeApiPlugin>(pluginCtx);

            // assert

            var account = _context.CreateQuery<Account>().FirstOrDefault();

            Assert.NotNull(account);
            Assert.True(account.Attributes.ContainsKey("name"));
            Assert.True(account.Name == expectedName);
            Assert.True(pluginCtx.OutputParameters.ContainsKey("accountId"));
            Assert.Equal(pluginCtx.OutputParameters["accountId"], account.Id);

        }

        [Fact]
        public void api_plugin_should_work_standalone_via_IGenericFakeMessageExecutor()
        {
            // arrange

            var initialName = "Initial Name";
            var expectedName = initialName;

            var request = new OrganizationRequest(FakeApiPlugin.Message)
            {
                Parameters = new ParameterCollection()
            };
            request.Parameters.AddOrUpdateIfNotNull("name",initialName );

            // act
            var result = _service.Execute(request);

            // assert

            var account = _context.CreateQuery<Account>().FirstOrDefault();

            Assert.NotNull(account);
            Assert.True(account.Attributes.ContainsKey("name"));
            Assert.True(account.Name == expectedName);
            
        }

        /*
         * This is the failing test that demonstrates
         * pipeline simulation not working for
         * custom message type
         *
         */
        [Theory]
        [InlineData(ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepMode.Asynchronous)]
        public void follow_on_plugin_should_be_called_by_pipeline_simulation(ProcessingStepMode followingPluginMode)
        {
            // arrange

            var initialName = "Initial Name";
            var expectedName = "Updated Name";

            _context.RegisterPluginStep<FakeApiPostOperationPlugin>(FakeApiPlugin.Message, ProcessingStepStage.Postoperation, followingPluginMode);

            var request = new OrganizationRequest(FakeApiPlugin.Message)
            {
                Parameters = new ParameterCollection()
            };
            request.Parameters.AddOrUpdateIfNotNull("name", initialName);

            // act
            var result = _service.Execute(request);

            // assert

            // assert 1 - check second plugin was called
            var pluginStepAudit = _context.GetPluginStepAudit();
            var stepsAudit = pluginStepAudit.CreateQuery().ToList();
            Assert.Single(stepsAudit);
            var auditedStep = stepsAudit[0];
            Assert.Equal(FakeApiPlugin.Message, auditedStep.MessageName);
            Assert.Equal(ProcessingStepStage.Postoperation, auditedStep.Stage);
            Assert.Equal(typeof(FakeApiPostOperationPlugin), auditedStep.PluginAssemblyType);

            // assert 2 - check operation of second plugin
            var account = _context.CreateQuery<Account>().FirstOrDefault();
            Assert.NotNull(account);
            Assert.True(account.Attributes.ContainsKey("name"));
            Assert.True(account.Name == expectedName);



        }
    }

    /*
     *   Plugin classes for test
     */

    /// <summary>
    /// This would be configured in make.powerapps.com as the plugin for the custom API
    /// </summary>
    public class FakeApiPlugin : IPlugin
    {
        public static string Message = "FakeApiMessage";
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            var pluginContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            var serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            
            var svc = serviceFactory.CreateOrganizationService(pluginContext.UserId);

            if (pluginContext.MessageName != Message)
            {
                throw new InvalidPluginExecutionException($"{nameof(FakeApiPlugin)} registered incorrectly");
            }

            if (!pluginContext.InputParameters.TryGetValue("name", out var accountName))
            {
                throw new InvalidPluginExecutionException($"{nameof(FakeApiPlugin)} cannot find parameter 'name' in input");
            };

            var account = new Account()
            {
                Id = Guid.NewGuid(),
                Name = (string) accountName
            };

            svc.Create(account);

            pluginContext.OutputParameters.AddOrUpdateIfNotNull("accountId", account.Id);}

        
    }

    /// <summary>
    /// Registered to be called Post Operation on our custom API message - in real life usually to do some lengthy processing
    /// </summary>
    public class FakeApiPostOperationPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            var pluginContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            var serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            var svc = serviceFactory.CreateOrganizationService(pluginContext.UserId);

            if (pluginContext.MessageName != FakeApiPlugin.Message)
            {
                throw new InvalidPluginExecutionException($"{nameof(FakeApiPostOperationPlugin)} registered incorrectly");
            }

            if (!pluginContext.InputParameters.TryGetValue("name", out var accountName))
            {
                throw new InvalidPluginExecutionException($"{nameof(FakeApiPostOperationPlugin)} cannot find parameter 'name' in input");
            };

            if (!pluginContext.OutputParameters.TryGetValue("accountId", out var accountId))
            {
                throw new InvalidPluginExecutionException($"{nameof(FakeApiPostOperationPlugin)} cannot find parameter 'accountId' in output from upstream");
            };

            var account = svc.Retrieve("account", (Guid) accountId, new ColumnSet(true))?.ToEntity<Account>();

            if (account == null)
            {
                throw new InvalidPluginExecutionException("Cannot find account created by upstream plugin");
            }

            var toUpdate = new Account
            {
                Id = account.Id,
                Name = "Updated Name"
            };

            svc.Update(toUpdate);
        }
    }


    /*
     * Fake message executor
     */
    /// <summary>
    /// This fakes mapping of custom message to a call 
    /// </summary>
    public class CustomApiRequestExecutor : IGenericFakeMessageExecutor
    {

        public static readonly string RequestName = FakeApiPlugin.Message;
        public bool CanExecute(OrganizationRequest request)
        {
            return request.RequestName.Equals(RequestName);
        }

        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {

            var response = new OrganizationResponse()
            {
                // Response name should be equal with Request name to check if the response is correct.
                ResponseName = RequestName,
                Results = new ParameterCollection()
            };

            var pluginCtx = ctx.GetDefaultPluginContext();
            pluginCtx.MessageName = request.RequestName;
            pluginCtx.Stage = 30;
            pluginCtx.InputParameters = new ParameterCollection();
            pluginCtx.InputParameters.AddOrUpdateIfNotNull("name", request.Parameters["name"]);

            ctx.ExecutePluginWith<FakeApiPlugin>(pluginCtx);

            var outputAccountId = pluginCtx.OutputParameters["accountId"];

            response.Results.AddOrUpdateIfNotNull("accountId", outputAccountId);

            return response;
        }

        public Type GetResponsibleRequestType()
        {
            return typeof(OrganizationRequest);
        }

        public string GetRequestName()
        {
            return RequestName;
        }
    }
}