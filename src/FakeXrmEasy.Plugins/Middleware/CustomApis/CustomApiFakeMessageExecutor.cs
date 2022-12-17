using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums.CustomApis;
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
    public class CustomApiFakeMessageExecutor<T, R> : ICustomApiFakeMessageExecutor where T : IPlugin, new() where R : OrganizationRequest, new()
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
        }

        /// <summary>
        /// The name of the request that will trigger this custom api
        /// </summary>
        public string MessageName
        {
            get { return _request.RequestName; }
        }

        /// <summary>
        /// Custom Processing Type of this CustomApiFakeMessageExecutor
        /// </summary>
        public virtual CustomProcessingStepType CustomProcessingType => CustomProcessingStepType.None;

        /// <summary>
        /// Binding type for this CustomApiFakeMessageExecutor
        /// </summary>
        public virtual BindingType BindingType => BindingType.Global;

        /// <summary>
        /// If BindingType is Entity, this is the name of the Entity to bind this custom api to 
        /// </summary>
        public virtual string EntityLogicalName => null;

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

            pluginContext.MessageName = MessageName;
            pluginContext.InputParameters = request.Parameters;
            ctx.ExecutePluginWith(pluginContext, _pluginType);

            return new OrganizationResponse()
            {
                Results = pluginContext.OutputParameters
            };
        }
    }

    /// <summary>
    /// Default custom api executor that associates a late bound organization request with a plugin type that will implement it
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CustomApiFakeMessageExecutor<T> : ICustomApiFakeMessageExecutor where T : IPlugin, new()
    {
        /// <summary>
        /// The plugin assembly to execute by this message
        /// </summary>
        protected readonly T _pluginType;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CustomApiFakeMessageExecutor()
        {
            _pluginType = new T();
        }

        /// <summary>
        /// Returns the plugin instance that will execute this custom api
        /// </summary>
        public IPlugin PluginType
        {
            get { return _pluginType; }
        }

        /// <summary>
        /// The name of the request that will trigger this custom api
        /// </summary>
        public virtual string MessageName
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Custom Processing Type of this CustomApiFakeMessageExecutor
        /// </summary>
        public virtual CustomProcessingStepType CustomProcessingType => CustomProcessingStepType.None;

        /// <summary>
        /// Binding type for this CustomApiFakeMessageExecutor
        /// </summary>
        public virtual BindingType BindingType => BindingType.Global;

        /// <summary>
        /// If BindingType is Entity, this is the name of the Entity to bind this custom api to 
        /// </summary>
        public virtual string EntityLogicalName => null;

        /// <summary>
        /// Returns the organization request type associated with this custom api
        /// </summary>
        /// <returns></returns>
        public virtual Type GetResponsibleRequestType()
        {
            return typeof(OrganizationRequest);
        }

        /// <summary>
        /// Returns true if the custom api executor can execute the specified request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual bool CanExecute(OrganizationRequest request)
        {
            return request.RequestName.Equals(MessageName);
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

            pluginContext.MessageName = MessageName;
            pluginContext.InputParameters = request.Parameters;
            ctx.ExecutePluginWith(pluginContext, _pluginType);

            return new OrganizationResponse()
            {
                Results = pluginContext.OutputParameters
            };
        }
    }
}
