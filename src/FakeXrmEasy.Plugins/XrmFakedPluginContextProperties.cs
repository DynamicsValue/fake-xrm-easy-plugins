
using FakeItEasy;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Exceptions;
using FakeXrmEasy.Abstractions.Plugins;
using Microsoft.Xrm.Sdk;
#if FAKE_XRM_EASY_9
using Microsoft.Xrm.Sdk.PluginTelemetry;
using FakeXrmEasy.Plugins.PluginExecutionContext;
#endif
using System;


namespace FakeXrmEasy.Plugins 
{
    /// <summary>
    /// Implements several interfaces that are needed for plugin execution (i.e. IServiceProvider, IExecutionContext, IPluginExecutionContext)
    /// from a given IOrganizationService and ITracingService
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

#if FAKE_XRM_EASY_9
                   if (t == typeof(IPluginExecutionContext2))
                   {
                       return GetFakedPluginContext2((XrmFakedPluginExecutionContext2) plugCtx);
                   }
                   
                   if (t == typeof(IPluginExecutionContext3))
                   {
                       return GetFakedPluginContext3((XrmFakedPluginExecutionContext3) plugCtx);
                   }
                   
                   if (t == typeof(IPluginExecutionContext4))
                   {
                       return GetFakedPluginContext4((XrmFakedPluginExecutionContext4) plugCtx);
                   }
                   
                   if (t == typeof(IPluginExecutionContext5))
                   {
                       return GetFakedPluginContext5((XrmFakedPluginExecutionContext5) plugCtx);
                   }
                   
                   if (t == typeof(IPluginExecutionContext6))
                   {
                       return GetFakedPluginContext6((XrmFakedPluginExecutionContext6) plugCtx);
                   }
                   
                   if (t == typeof(IPluginExecutionContext7))
                   {
                       return GetFakedPluginContext7((XrmFakedPluginExecutionContext7) plugCtx);
                   }
#endif
                   
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
                   return null;
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

            PopulatePluginExecutionContextPropertiesFromFakedContext(context, ctx);

            return context;
        }

        #if FAKE_XRM_EASY_9
        /// <summary>
        /// Returns a fake plugin execution context for xMultiple messages from a default plugin context in code
        /// </summary>
        /// <param name="ctx">The fake plugin context with support for xMultiple messages</param>
        /// <returns></returns>
        protected IPluginExecutionContext2 GetFakedPluginContext2(XrmFakedPluginExecutionContext2 ctx)
        {
            var context = A.Fake<IPluginExecutionContext2>();

            PopulatePluginExecutionContextPropertiesFromFakedContext2(context, ctx);
            
            return context;
        }
        
        /// <summary>
        /// Returns a fake plugin execution context3
        /// </summary>
        /// <param name="ctx">The default plugin context 3 class</param>
        /// <returns></returns>
        protected IPluginExecutionContext3 GetFakedPluginContext3(XrmFakedPluginExecutionContext3 ctx)
        {
            var context = A.Fake<IPluginExecutionContext3>();

            PopulatePluginExecutionContextPropertiesFromFakedContext3(context, ctx);

            return context;
        }
        
        /// <summary>
        /// Returns a fake plugin execution context for xMultiple messages from a default plugin context in code
        /// </summary>
        /// <param name="ctx">The fake plugin context with support for xMultiple messages</param>
        /// <returns></returns>
        protected IPluginExecutionContext4 GetFakedPluginContext4(XrmFakedPluginExecutionContext4 ctx)
        {
            var context = A.Fake<IPluginExecutionContext4>();

            PopulatePluginExecutionContextPropertiesFromFakedContext4(context, ctx);
            
            return context;
        }
        
        /// <summary>
        /// Returns a fake plugin execution context5
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        protected IPluginExecutionContext5 GetFakedPluginContext5(XrmFakedPluginExecutionContext5 ctx)
        {
            var context = A.Fake<IPluginExecutionContext5>();

            PopulatePluginExecutionContextPropertiesFromFakedContext5(context, ctx);
            
            return context;
        }
        
        protected IPluginExecutionContext6 GetFakedPluginContext6(XrmFakedPluginExecutionContext6 ctx)
        {
            var context = A.Fake<IPluginExecutionContext6>();

            PopulatePluginExecutionContextPropertiesFromFakedContext6(context, ctx);
            
            return context;
        }
        
        protected IPluginExecutionContext7 GetFakedPluginContext7(XrmFakedPluginExecutionContext7 ctx)
        {
            var context = A.Fake<IPluginExecutionContext7>();

            PopulatePluginExecutionContextPropertiesFromFakedContext7(context, ctx);
            
            return context;
        }
        #endif
        
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

        /// <summary>
        /// Populates IPluginExecutionContext properties from a XrmFakedPluginExecutionContext
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ctx"></param>
        protected void PopulatePluginExecutionContextPropertiesFromFakedContext(IPluginExecutionContext context,
            XrmFakedPluginExecutionContext ctx)
        {
            PopulateExecutionContextPropertiesFromFakedContext(context, ctx);
            A.CallTo(() => context.ParentContext).ReturnsLazily(() => ctx.ParentContext);
            A.CallTo(() => context.Stage).ReturnsLazily(() => ctx.Stage);
        }
        
        /// <summary>
        /// Populates IPluginExecutionContext2 properties from a XrmFakedPluginExecutionContext2
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ctx"></param>
        protected void PopulatePluginExecutionContextPropertiesFromFakedContext2(IPluginExecutionContext2 context,
            XrmFakedPluginExecutionContext2 ctx)
        {
            PopulateExecutionContextPropertiesFromFakedContext(context, ctx);
            A.CallTo(() => context.UserAzureActiveDirectoryObjectId).ReturnsLazily(() => ctx.UserAzureActiveDirectoryObjectId);
            A.CallTo(() => context.InitiatingUserAzureActiveDirectoryObjectId).ReturnsLazily(() => ctx.InitiatingUserAzureActiveDirectoryObjectId);
            A.CallTo(() => context.InitiatingUserApplicationId).ReturnsLazily(() => ctx.InitiatingUserApplicationId);
            A.CallTo(() => context.PortalsContactId).ReturnsLazily(() => ctx.PortalsContactId);
            A.CallTo(() => context.IsPortalsClientCall).ReturnsLazily(() => ctx.IsPortalsClientCall);
        }
        
        /// <summary>
        /// Populates IPluginExecutionContext3 properties from a XrmFakedPluginExecutionContext3
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ctx"></param>
        protected void PopulatePluginExecutionContextPropertiesFromFakedContext3(IPluginExecutionContext3 context,
            XrmFakedPluginExecutionContext3 ctx)
        {
            PopulatePluginExecutionContextPropertiesFromFakedContext2(context, ctx);
            A.CallTo(() => context.AuthenticatedUserId).ReturnsLazily(() => ctx.AuthenticatedUserId);
        }
        
        /// <summary>
        /// Populates IPluginExecutionContext4 properties from a XrmFakedPluginExecutionContext4
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ctx"></param>
        protected void PopulatePluginExecutionContextPropertiesFromFakedContext4(IPluginExecutionContext4 context,
            XrmFakedPluginExecutionContext4 ctx)
        {
            PopulatePluginExecutionContextPropertiesFromFakedContext3(context, ctx);
            A.CallTo(() => context.PreEntityImagesCollection).ReturnsLazily(() => ctx.PreEntityImagesCollection);
            A.CallTo(() => context.PostEntityImagesCollection).ReturnsLazily(() => ctx.PostEntityImagesCollection);
        }
        
        /// <summary>
        /// Populates IPluginExecutionContext5 properties from a XrmFakedPluginExecutionContext5
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ctx"></param>
        protected void PopulatePluginExecutionContextPropertiesFromFakedContext5(IPluginExecutionContext5 context,
            XrmFakedPluginExecutionContext5 ctx)
        {
            PopulatePluginExecutionContextPropertiesFromFakedContext4(context, ctx);
            A.CallTo(() => context.InitiatingUserAgent).ReturnsLazily(() => ctx.InitiatingUserAgent);
        }
        
        /// <summary>
        /// Populates IPluginExecutionContext6 properties from a XrmFakedPluginExecutionContext6
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ctx"></param>
        protected void PopulatePluginExecutionContextPropertiesFromFakedContext6(IPluginExecutionContext6 context,
            XrmFakedPluginExecutionContext6 ctx)
        {
            PopulatePluginExecutionContextPropertiesFromFakedContext5(context, ctx);
            A.CallTo(() => context.EnvironmentId).ReturnsLazily(() => ctx.EnvironmentId);
            A.CallTo(() => context.TenantId).ReturnsLazily(() => ctx.TenantId);
        }
        
        /// <summary>
        /// Populates IPluginExecutionContext7 properties from a XrmFakedPluginExecutionContext7
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ctx"></param>
        protected void PopulatePluginExecutionContextPropertiesFromFakedContext7(IPluginExecutionContext7 context,
            XrmFakedPluginExecutionContext7 ctx)
        {
            PopulatePluginExecutionContextPropertiesFromFakedContext6(context, ctx);
            A.CallTo(() => context.IsApplicationUser).ReturnsLazily(() => ctx.IsApplicationUser);
        }
    }
}