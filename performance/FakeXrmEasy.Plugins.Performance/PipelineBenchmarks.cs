using BenchmarkDotNet.Attributes;
using Crm;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.Performance.PluginsForTesting;
using FakeXrmEasy.Plugins.PluginImages;
using FakeXrmEasy.Plugins.PluginSteps;
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
                _lastPluginStepId = _context.RegisterPluginStep<AccountNumberPlugin>(new PluginStepDefinition()
                {
                    MessageName = "Create",
                    Stage = ProcessingStepStage.Postoperation,
                    Mode = ProcessingStepMode.Synchronous,
                    Rank = i + 1,
                    ImagesDefinitions = new PluginImageDefinition[] { preImageDefinition }
                });
            }
        }

        /* Significantly slower than GetPluginStepsWithQuery below
        [Benchmark]
        public void GetPluginStepsWithRetrieveMultiple()
        {
            RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequestWithRetrieveMultiple(_context, "Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous, _createRequest);
        }
        */
        
        [Benchmark]
        public void GetPluginStepsWithQuery()
        {
            RegisteredPluginStepsRetriever.GetPluginStepsForOrganizationRequest(_context, "Create", ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous, _createRequest);
        }

        [Benchmark]
        public void GetPluginStepImageWithRetrieveMultiple()
        {
            RegisteredPluginStepsRetriever.GetPluginImageDefinitionsWithRetrieveMultiple(_context, _lastPluginStepId, ProcessingStepImageType.PostImage).ToList();
        }

        [Benchmark]
        public void GetPluginStepImageWithQuery()
        {
            RegisteredPluginStepsRetriever.GetPluginImageDefinitionsWithQuery(_context, _lastPluginStepId, ProcessingStepImageType.PostImage).ToList();
        }
    }
}
