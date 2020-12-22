using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Crud;
using FakeXrmEasy.Middleware.Messages;
using FakeXrmEasy.Middleware.Pipeline;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Plugins.Tests
{
    public class FakeXrmEasyPipelineTests
    {
        protected readonly IXrmFakedContext _context;
        protected readonly IOrganizationService _service;

        public FakeXrmEasyPipelineTests()
        {
            _context = MiddlewareBuilder
                        .New()
       
                        // Add* -> Middleware configuration
                        .AddCrud()   
                        .AddFakeMessageExecutors()
                        .AddPipelineSimulation()

                        // Use* -> Defines pipeline sequence
                        .UsePipelineSimulation()
                        .UseCrud() 
                        .UseMessages()

                        .Build();
                        
            _service = _context.GetOrganizationService();
        }
    }
}
