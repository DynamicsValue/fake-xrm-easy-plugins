using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace FakeXrmEasy.Plugins.Performance
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = DefaultConfig.Instance.With(Job.Default.WithCustomBuildConfiguration("FAKE_XRM_EASY_9"))
                .With(ConfigOptions.DisableOptimizationsValidator);
            //config.WithOption(ConfigOptions.DisableOptimizationsValidator, true);
            
            //Job.Default.WithCustomBuildConfiguration("FAKE_XRN_EASY_9");
            //Job.Default.WithMsBuildArguments("/p:NuGetRestorePackages=false /p:Configuration=FAKE_XRM_EASY_9 /p:TargetFrameworkVersion=net10.0");
            BenchmarkRunner.Run(typeof(PipelineBenchmarks), config);
        }
    }
}