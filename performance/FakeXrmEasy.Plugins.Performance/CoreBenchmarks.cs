using BenchmarkDotNet.Attributes;
using Crm;

namespace FakeXrmEasy.Plugins.Performance
{
    public class CoreBenchmarks: FakeXrmEasyBenchmarksBase
    {
        public CoreBenchmarks()
        {
            
        }
        
        [Benchmark]
        public void NewFindReflectedType()
        {
            for (var i = 0; i < 1000; i++)
            {
                _context.FindReflectedType(Account.EntityLogicalName);
            }
        }
    }
}
