using System;
using System.ServiceModel;
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Plugins.Tests
{
    public static class XAssert
    {
        public static FaultException<OrganizationServiceFault> ThrowsFaultCode(ErrorCodes errorCode, Action testCode) 
        {
            var exception = Xunit.Assert.Throws<FaultException<OrganizationServiceFault>>(testCode);
            Xunit.Assert.Equal((int)errorCode, exception.Detail.ErrorCode);
            return exception;
        }
        
        public static FaultException<OrganizationServiceFault> ThrowsFaultCode(ErrorCodes errorCode, Func<object> testCode) 
        {
            var exception = Xunit.Assert.Throws<FaultException<OrganizationServiceFault>>(testCode);
            Xunit.Assert.Equal((int)errorCode, exception.Detail.ErrorCode);
            return exception;
        }
    }
}