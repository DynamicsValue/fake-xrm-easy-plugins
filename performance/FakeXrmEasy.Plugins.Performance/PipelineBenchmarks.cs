using BenchmarkDotNet.Attributes;
using Crm;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.Performance.PluginsForTesting;
using FakeXrmEasy.Plugins.PluginImages;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Linq;

namespace FakeXrmEasy.Plugins.Performance
{
    public class PipelineBenchmarks: FakeXrmEasyBenchmarksBase
    {
        private readonly CreateRequest _createRequest;
        private readonly Account _target;
        private Guid _lastPluginStepId;
        public PipelineBenchmarks()
        {
            _target = new Account() { };

            _createRequest = new CreateRequest()
            {
                Target = _target
            };

            string registeredPreImageName = "PostImage";
            PluginImageDefinition preImageDefinition = new PluginImageDefinition(registeredPreImageName, ProcessingStepImageType.PostImage);

            for (var i = 0; i < 1000; i++)
            {
                _lastPluginStepId = _context.RegisterPluginStep<AccountNumberPlugin>("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous, rank: i + 1, registeredImages: new PluginImageDefinition[] { preImageDefinition });
            }
        }

        [Benchmark]
        public void GetPluginStepsWithRetrieveMultiple()
        {
            _context.GetPluginStepsForOrganizationRequestWithRetrieveMultiple("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous, _createRequest);
        }

        [Benchmark]
        public void GetPluginStepsWithQuery()
        {
            _context.GetPluginStepsForOrganizationRequest("Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous, _createRequest);
        }

        [Benchmark]
        public void GetPluginStepImageWithRetrieveMultiple()
        {
            _context.GetPluginImageDefinitionsWithRetrieveMultiple(_lastPluginStepId, ProcessingStepImageType.PostImage).ToList();
        }

        [Benchmark]
        public void GetPluginStepImageWithQuery()
        {
            _context.GetPluginImageDefinitionsWithQuery(_lastPluginStepId, ProcessingStepImageType.PostImage).ToList();
        }
    }
}
