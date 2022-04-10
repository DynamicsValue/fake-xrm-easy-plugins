using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Middleware;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Plugins.Performance
{
    public class FakeXrmEasyBenchmarksBase
    {
        protected readonly IXrmFakedContext _context;
        protected readonly IOrganizationService _service;

        public FakeXrmEasyBenchmarksBase()
        {
            _context = XrmFakedContextFactory.New(FakeXrmEasyLicense.RPL_1_5);
            _service = _context.GetOrganizationService();
        }
    }
}
