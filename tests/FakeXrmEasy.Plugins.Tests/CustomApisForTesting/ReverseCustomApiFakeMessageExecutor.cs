using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Plugins.Middleware.CustomApis;

namespace FakeXrmEasy.Plugins.Tests.CustomApisForTesting
{
    /// <summary>
    /// Generic Custom Api
    /// </summary>
    public class ReverseCustomApiFakeMessageExecutor : CustomApiFakeMessageExecutor<ReverseCustomApiPlugin>, ICustomApiFakeMessageExecutor
    {
        public override string MessageName
        {
            get { return "sample_CustomAPIExample"; }
            set { ; }
        }
    }
}

