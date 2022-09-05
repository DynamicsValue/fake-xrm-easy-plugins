#if FAKE_XRM_EASY_9
using FakeItEasy;
using FakeXrmEasy.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Services
{
    public class ILoggerTests : FakeXrmEasyTestsBase
    {
        [Fact]
        public void Should_implement_logger_interface()
        {
            _context.ExecutePluginWith<LoggerPlugin>();

            var logger = _context.PluginContextProperties.Logger;

            A.CallTo(() => logger.LogInformation("Test")).MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public void Should_use_overriden_logger_implementation_when_set()
        {
            _context.PluginContextProperties = new XrmFakedPluginContextProperties(_context, _context.GetOrganizationService(), _context.GetTracingService());
            var defaultLogger = _context.PluginContextProperties.Logger;

            var myLogger = A.Fake<ILogger>();
            _context.PluginContextProperties.Logger = myLogger;

            _context.ExecutePluginWith<LoggerPlugin>();

            A.CallTo(() => myLogger.LogInformation("Test")).MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => defaultLogger.LogInformation("Test")).MustNotHaveHappened();
        }
    }
}
#endif
