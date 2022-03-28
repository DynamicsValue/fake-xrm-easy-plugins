using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace FakeXrmEasy.Plugins.Tests.Benchmarks
{
    public class BenchmarkTests: FakeXrmEasyTestsBase
    {
        private readonly ITestOutputHelper _output;

        public BenchmarkTests(ITestOutputHelper output)
        {
            _output = output;
        }

        /*  Lowering the priority of this for 
        [Benchmark]
        public string Cosa() => "Cosa".ToLowerInvariant();


        [Fact]
        public void Run_benchmarks()
        {
            _output.WriteLine("Test Run");
            var summary = BenchmarkRunner.Run(typeof(BenchmarkTests));
            var summaryTable = summary.Table;

            StringBuilder sBuilder = new StringBuilder();
            var methodNameIndex = 0;
            var meanIndex = summaryTable.FullHeader.Length - 1;

            _output.WriteLine("Method|Mean");

            foreach(var row in summaryTable.FullContent)
            {
                _output.WriteLine($"{row[methodNameIndex]}|{row[meanIndex]}");
            }
            
        }
        */
    }
}
