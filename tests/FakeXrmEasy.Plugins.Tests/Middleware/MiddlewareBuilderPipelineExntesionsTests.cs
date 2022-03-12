using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Pipeline;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Middleware
{
    public class MiddlewareBuilderPipelineExntesionsTests
    {
        [Fact]
        public void Should_enable_pipeline_simulation_when_add_pipeline_simulation_is_called()
        {
            var context = MiddlewareBuilder
                        .New()
                        .AddPipelineSimulation()
                        .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                        .Build();

            Assert.True(context.GetProperty<PipelineOptions>().UsePipelineSimulation);
        }
    }
}
