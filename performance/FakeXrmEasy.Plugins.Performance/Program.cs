using BenchmarkDotNet.Running;

namespace FakeXrmEasy.Plugins.Performance
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run(typeof(PipelineBenchmarks));
        }
    }
}