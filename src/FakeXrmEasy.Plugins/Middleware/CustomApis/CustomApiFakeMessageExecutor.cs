using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using Microsoft.Xrm.Sdk;
using System;

namespace FakeXrmEasy.Plugins.Middleware.CustomApis
{
    /// <summary>
    /// Default custom api executor that associates an early bound organization request with a plugin type that will implement it
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="R"></typeparam>
    public class CustomApiFakeMessageExecutor<T, R>: ICustomApiFakeMessageExecutor where T: IPlugin, new() where R: OrganizationRequest, new()
    {
        private readonly T _pluginType;
        private readonly R _request;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CustomApiFakeMessageExecutor()
        {
            _pluginType = new T();
            _request = new R();
        }

        /// <summary>
        /// Returns the plugin instance that will execute this custom api
        /// </summary>
        public IPlugin PluginType 
        {
            get { return _pluginType; }
            set { ; }
        }

        /// <summary>
        /// Returns the organization request type associated with this custom api
        /// </summary>
        /// <returns></returns>
        public Type GetResponsibleRequestType()
        {
            return _request.GetType();
        }

        /// <summary>
        /// Returns true if the custom api executor can execute the specified request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool CanExecute(OrganizationRequest request)
        {
            return request.GetType() == _request.GetType();
        }

        /// <summary>
        /// Executes the custom api
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            if (_pluginType == null) return new OrganizationResponse();

            var pluginContext = ctx.GetDefaultPluginContext();
            pluginContext.Stage = (int)ProcessingStepStage.MainOperation;

            //TO DO: Should clone these....
            pluginContext.InputParameters = request.Parameters;
            ctx.ExecutePluginWith(pluginContext, _pluginType);

            return new OrganizationResponse()
            {
                Results = pluginContext.OutputParameters // TO DO: Should also clone these....
            };
        }
    }
}
