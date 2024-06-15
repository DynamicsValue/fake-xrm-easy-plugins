using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Crud;
using FakeXrmEasy.Middleware.Messages;
using FakeXrmEasy.Middleware.Pipeline;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Plugins.Tests
{
    public class FakeXrmEasyPipelineWithAuditTestsBase
    {
        protected readonly IXrmFakedContext _context;
        protected readonly IOrganizationService _service;

        public FakeXrmEasyPipelineWithAuditTestsBase()
        {
            _context = MiddlewareBuilder
                        .New()
       
                        // Add* -> Middleware configuration
                        .AddCrud()   
                        .AddFakeMessageExecutors()
                        .AddGenericFakeMessageExecutors()

                        .AddPipelineSimulation(new PipelineOptions() { UsePluginStepAudit = true})

                        // Use* -> Defines pipeline sequence
                        .UsePipelineSimulation()
                        .UseCrud() 
                        .UseMessages()

                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();
                        
            _service = _context.GetOrganizationService();
        }
    }
}
