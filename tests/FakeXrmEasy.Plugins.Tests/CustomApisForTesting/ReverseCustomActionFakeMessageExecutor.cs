using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using Microsoft.Xrm.Sdk;
using System;

namespace FakeXrmEasy.Plugins.Tests.CustomApisForTesting
{
    /// <summary>
    /// Generic Fake Message Executor
    /// </summary>
    public class ReverseCustomActionFakeMessageExecutor : IGenericFakeMessageExecutor
    {
        public bool CanExecute(OrganizationRequest request)
        {
            return request.RequestName == GetRequestName();
        }

        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            return new OrganizationResponse();
        }

        public string GetRequestName()
        {
            return "sample_CustomAPIExample";
        }

        public Type GetResponsibleRequestType()
        {
            return typeof(OrganizationRequest); //late bound
        }
    }
}

