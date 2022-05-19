using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;

namespace FakeXrmEasy.Plugins.Performance
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = DefaultConfig.Instance.With(ConfigOptions.DisableOptimizationsValidator);
            var summary = BenchmarkRunner.Run(typeof(PipelineBenchmarks), config);
        }
    }
}