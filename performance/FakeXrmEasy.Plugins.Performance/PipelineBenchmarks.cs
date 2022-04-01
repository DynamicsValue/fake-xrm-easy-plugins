using BenchmarkDotNet.Attributes;
using Crm;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.Performance.PluginsForTesting;
using Microsoft.Xrm.Sdk.Messages;

namespace FakeXrmEasy.Plugins.Performance
{
    public class PipelineBenchmarks: FakeXrmEasyBenchmarksBase
    {
        private readonly CreateRequest _createRequest;
        private readonly Account _target;

        public PipelineBenchmarks()
        {
            _target = new Account() { };

            _createRequest = new CreateRequest()
            {
                Target = _target
            };

            for (var i = 0; i < 1000; i++)
            {
                _context.RegisterPluginStep<AccountNumberPlugin>("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous, rank: i + 1);
            }
        }

        [Benchmark]
        public void GetPluginStepsWithRetrieveMultiple()
        {
            _context.GetPluginStepsForOrganizationRequestWithRetrieveMultiple("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous, _createRequest);
        }

        [Benchmark]
        public void GetPluginStepsWithoutRetrieveMultiple()
        {
            _context.GetPluginStepsForOrganizationRequest("Create", ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous, _createRequest);
        }
    }
}
