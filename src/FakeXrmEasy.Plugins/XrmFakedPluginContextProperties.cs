
using FakeItEasy;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Exceptions;
using FakeXrmEasy.Abstractions.Plugins;
using Microsoft.Xrm.Sdk;
#if FAKE_XRM_EASY_9
using Microsoft.Xrm.Sdk.PluginTelemetry;
#endif
using System;

namespace FakeXrmEasy.Plugins 
{
    /// <summary>
    /// Implementation to override default plugin context properties
    /// </summary>
    public class XrmFakedPluginContextProperties : IXrmFakedPluginContextProperties
    {
        /// <summary>
        /// Reference to the IXrmBaseContext that created this instance
        /// </summary>
        protected readonly IXrmBaseContext _context;

        /// <summary>
        /// A fake organization service
        /// </summary>
        protected readonly IOrganizationService _service;

        /// <summary>
        /// A fake tracing service
        /// </summary>
        protected readonly IXrmFakedTracingService _tracingService;

#if FAKE_XRM_EASY_9
        protected readonly IEntityDataSourceRetrieverService _entityDataSourceRetrieverService;
        /// <summary>
        /// Plugin telemetry logger service
        /// </summary>
        protected ILogger _loggerService;
#endif

        protected readonly IOrganizationServiceFactory _organizationServiceFactory;
        protected readonly IServiceEndpointNotificationService _serviceEndpointNotificationService;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="context">The IXrmBaseContext from which this instance is created</param>
        /// <param name="service">The fake organization service to use by this plugin context properties instance</param>
        /// <param name="tracingService">The fake tracing service to use by this plugin context properties instance</param>
        public XrmFakedPluginContextProperties(IXrmBaseContext context, IOrganizationService service, IXrmFakedTracingService tracingService) 
        {
            _context = context;
            _service = service;
            _tracingService = tracingService;

            _organizationServiceFactory = A.Fake<IOrganizationServiceFactory>();
            A.CallTo(() => _organizationServiceFactory.CreateOrganizationService(A<Guid?>._)).ReturnsLazily((Guid? g) => _service);

            _serviceEndpointNotificationService = A.Fake<IServiceEndpointNotificationService>();

#if FAKE_XRM_EASY_9
                _entityDataSourceRetrieverService = A.Fake<IEntityDataSourceRetrieverService>();
                A.CallTo(() => _entityDataSourceRetrieverService.RetrieveEntityDataSource())
                    .ReturnsLazily(() => EntityDataSourceRetriever);

            _loggerService = A.Fake<ILogger>();
#endif
        }


        public IOrganizationService OrganizationService => _service;
        public IXrmFakedTracingService TracingService => _tracingService;
        #if FAKE_XRM_EASY_9
        public IEntityDataSourceRetrieverService EntityDataSourceRetrieverService => _entityDataSourceRetrieverService;
        #endif

        public IOrganizationServiceFactory OrganizationServiceFactory => _organizationServiceFactory;
        public IServiceEndpointNotificationService ServiceEndpointNotificationService => _serviceEndpointNotificationService;

#if FAKE_XRM_EASY_9
        /// <summary>
        /// Provides a default EntityDataSourceRetriever
        /// </summary>
        public Entity EntityDataSourceRetriever { get; set; }
   
        /// <summary>
        /// Provides a custom implementation for an ILogger interface or returns the current implementation
        /// </summary>
        public ILogger Logger { get => _loggerService; set => _loggerService = value; }
#endif

        /// <summary>
        /// Gets a fake service provider interface based on the current fake plugin execution context
        /// </summary>
        /// <param name="plugCtx"></param>
        /// <returns></returns>
        public IServiceProvider GetServiceProvider(IPluginExecutionContext plugCtx) 
        {
            var fakedServiceProvider = A.Fake<IServiceProvider>();

            A.CallTo(() => fakedServiceProvider.GetService(A<Type>._))
               .ReturnsLazily((Type t) =>
               {
                   if (t == typeof(IOrganizationService))
                   {
                       return _service;
                   }

                   if (t == typeof(ITracingService))
                   {
                       return _tracingService;
                   }

                   if (t == typeof(IPluginExecutionContext))
                   {
                       return GetFakedPluginContext((XrmFakedPluginExecutionContext) plugCtx);
                   }

                   if (t == typeof(IExecutionContext))
                   {
                       return GetFakedExecutionContext((XrmFakedPluginExecutionContext) plugCtx);
                   }

                   if (t == typeof(IOrganizationServiceFactory))
                   {
                       return _organizationServiceFactory;
                   }

                   if (t == typeof(IServiceEndpointNotificationService))
                   {
                       return _serviceEndpointNotificationService;
                   }

#if FAKE_XRM_EASY_9
                   if (t == typeof(ILogger))
                   {
                       return _loggerService;
                   }

                   if (t == typeof(IEntityDataSourceRetrieverService))
                   {
                       return _entityDataSourceRetrieverService;
                   }
#endif
                   throw UnsupportedExceptionFactory.New(_context.LicenseContext.Value, "The specified service type is not supported");
               });

            return fakedServiceProvider;
        
        }

        /// <summary>
        /// Returns a fake plugin execution context from a default plugin context in code
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        protected IPluginExecutionContext GetFakedPluginContext(XrmFakedPluginExecutionContext ctx)
        {
            var context = A.Fake<IPluginExecutionContext>();

            PopulateExecutionContextPropertiesFromFakedContext(context, ctx);

            A.CallTo(() => context.ParentContext).ReturnsLazily(() => ctx.ParentContext);
            A.CallTo(() => context.Stage).ReturnsLazily(() => ctx.Stage);

            return context;
        }

        
        /// <summary>
        /// Returns a fake execution context
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        protected IExecutionContext GetFakedExecutionContext(XrmFakedPluginExecutionContext ctx)
        {
            var context = A.Fake<IExecutionContext>();

            PopulateExecutionContextPropertiesFromFakedContext(context, ctx);

            return context;
        }

        /// <summary>
        /// Populates plugin context properties from a given fake plugin context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ctx"></param>
        protected void PopulateExecutionContextPropertiesFromFakedContext(IExecutionContext context, XrmFakedPluginExecutionContext ctx)
        {
            var newUserId = Guid.NewGuid();

            A.CallTo(() => context.BusinessUnitId).ReturnsLazily(() => ctx.BusinessUnitId);
            A.CallTo(() => context.CorrelationId).ReturnsLazily(() => ctx.CorrelationId);
            A.CallTo(() => context.Depth).ReturnsLazily(() => ctx.Depth <= 0 ? 1 : ctx.Depth);
            A.CallTo(() => context.InitiatingUserId).ReturnsLazily(() => ctx.InitiatingUserId == Guid.Empty ? newUserId : ctx.InitiatingUserId);
            A.CallTo(() => context.InputParameters).ReturnsLazily(() => ctx.InputParameters);
            A.CallTo(() => context.IsExecutingOffline).ReturnsLazily(() => ctx.IsExecutingOffline);
            A.CallTo(() => context.IsInTransaction).ReturnsLazily(() => ctx.IsInTransaction);
            A.CallTo(() => context.IsolationMode).ReturnsLazily(() => ctx.IsolationMode);
            A.CallTo(() => context.MessageName).ReturnsLazily(() => ctx.MessageName);
            A.CallTo(() => context.Mode).ReturnsLazily(() => ctx.Mode);
            A.CallTo(() => context.OperationCreatedOn).ReturnsLazily(() => ctx.OperationCreatedOn);
            A.CallTo(() => context.OrganizationId).ReturnsLazily(() => ctx.OrganizationId);
            A.CallTo(() => context.OrganizationName).ReturnsLazily(() => ctx.OrganizationName);
            A.CallTo(() => context.OutputParameters).ReturnsLazily(() => ctx.OutputParameters);
            A.CallTo(() => context.OwningExtension).ReturnsLazily(() => ctx.OwningExtension);
            A.CallTo(() => context.PostEntityImages).ReturnsLazily(() => ctx.PostEntityImages);
            A.CallTo(() => context.PreEntityImages).ReturnsLazily(() => ctx.PreEntityImages);
            A.CallTo(() => context.PrimaryEntityId).ReturnsLazily(() => ctx.PrimaryEntityId);
            A.CallTo(() => context.PrimaryEntityName).ReturnsLazily(() => ctx.PrimaryEntityName);
            A.CallTo(() => context.SecondaryEntityName).ReturnsLazily(() => ctx.SecondaryEntityName);
            A.CallTo(() => context.SharedVariables).ReturnsLazily(() => ctx.SharedVariables);
            A.CallTo(() => context.UserId).ReturnsLazily(() => ctx.UserId == Guid.Empty ? newUserId : ctx.UserId);
            
            // Create message will pass an Entity as the target but this is not always true
            // For instance, a Delete request will receive an EntityReference
            if (ctx.InputParameters != null && ctx.InputParameters.ContainsKey("Target"))
            {
                if (ctx.InputParameters["Target"] is Entity)
                {
                    var target = (Entity)ctx.InputParameters["Target"];
                    A.CallTo(() => context.PrimaryEntityId).ReturnsLazily(() => target.Id);
                    A.CallTo(() => context.PrimaryEntityName).ReturnsLazily(() => target.LogicalName);
                }
                else if (ctx.InputParameters["Target"] is EntityReference)
                {
                    var target = (EntityReference)ctx.InputParameters["Target"];
                    A.CallTo(() => context.PrimaryEntityId).ReturnsLazily(() => target.Id);
                    A.CallTo(() => context.PrimaryEntityName).ReturnsLazily(() => target.LogicalName);
                }
            }
        }
    }
}