using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using DataverseEntities;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.Audit;
using FakeXrmEasy.Plugins.PluginImages;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.Tests.PluginsForTesting;
using Microsoft.Crm.Sdk.Messages;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline.SendMessageTests
{
    public class SendTemplateTests: FakeXrmEasyPipelineWithAuditAndMessagesTestsBase
    {
        private readonly Template _template;
        private SendTemplateRequest _request;
        private readonly Contact _contact;
        private readonly Contact _contact2;
        private readonly Account _account;
        
        private const string ACCOUNT_NAME = "SendTemplateTest Organisation";
        
        private const string DUMMY_EMAIL = "jordi.montana+test@gmail.com";
        private const string DUMMY_EMAIL_2 = "jordi.montana+test2@gmail.com";
        
        private const string EMAIL_TEMPLATE_SUBJECT_XSLT =
            "<?xml version=\"1.0\" ?><xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\"><xsl:output method=\"text\" indent=\"no\" /><xsl:template match=\"/data\"><![CDATA[Thank you for registering with us]]></xsl:template></xsl:stylesheet>";
        private const string EMAIL_TEMPLATE_BODY_XSLT = "<?xml version=\"1.0\" ?><xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\"><xsl:output method=\"text\" indent=\"no\"/><xsl:template match=\"/data\"><![CDATA[<P>Dear ]]><xsl:choose><xsl:when test=\"contact/salutation\"><xsl:value-of select=\"contact/salutation\" /></xsl:when><xsl:otherwise></xsl:otherwise></xsl:choose><![CDATA[ ]]><xsl:choose><xsl:when test=\"contact/lastname\"><xsl:value-of select=\"contact/lastname\" /></xsl:when><xsl:otherwise>Valued Customer</xsl:otherwise></xsl:choose><![CDATA[  ,</P>\r\n     <P>Thank you for registering with us. We now have the following registration information on file:</P><P>Name: ]]><xsl:choose><xsl:when test=\"systemuser/fullname\"><xsl:value-of select=\"systemuser/fullname\" /></xsl:when><xsl:otherwise></xsl:otherwise></xsl:choose><![CDATA[<BR>Street Address: ]]><xsl:choose><xsl:when test=\"contact/address1_line1\"><xsl:value-of select=\"contact/address1_line1\" /></xsl:when><xsl:when test=\"contact/address1_line2\"><xsl:value-of select=\"contact/address1_line2\" /></xsl:when><xsl:when test=\"contact/address1_line3\"><xsl:value-of select=\"contact/address1_line3\" /></xsl:when><xsl:otherwise>No Address Provided</xsl:otherwise></xsl:choose><![CDATA[ <BR>City: ]]><xsl:choose><xsl:when test=\"contact/address1_city\"><xsl:value-of select=\"contact/address1_city\" /></xsl:when><xsl:otherwise></xsl:otherwise></xsl:choose><![CDATA[<BR>State or Province: ]]><xsl:choose><xsl:when test=\"contact/address1_stateorprovince\"><xsl:value-of select=\"contact/address1_stateorprovince\" /></xsl:when><xsl:otherwise></xsl:otherwise></xsl:choose><![CDATA[<BR>Country or Region: ]]><xsl:choose><xsl:when test=\"contact/address1_country\"><xsl:value-of select=\"contact/address1_country\" /></xsl:when><xsl:otherwise></xsl:otherwise></xsl:choose><![CDATA[<BR>Postal Code: ]]><xsl:choose><xsl:when test=\"contact/address1_postalcode\"><xsl:value-of select=\"contact/address1_postalcode\" /></xsl:when><xsl:otherwise></xsl:otherwise></xsl:choose><![CDATA[<BR>E-mail Address: ]]><xsl:choose><xsl:when test=\"contact/emailaddress1\"><xsl:value-of select=\"contact/emailaddress1\" /></xsl:when><xsl:otherwise></xsl:otherwise></xsl:choose><![CDATA[</P><P>If you would like to change or add additional information to your customer profile please visit our Web site. While there you can take advantage of the many self-service features of our site including scheduling, appointment notifications, knowledge base look-up, and service request management. </P><P>We look forward to serving you in the future.</P>\r\n     <P>Thank you.</P>]]></xsl:template></xsl:stylesheet>";
        private const string EMAIL_TEMPLATE_TITLE = "Thank you for registering with us";

        private const string EMAIL_RENDERED_BODY = @"<P>Dear Mr.Montana  ,</P>
     <P>Thank you for registering with us. We now have the following registration information on file:</P><P>Name: <BR>Street Address: No Address Provided <BR>City: <BR>State or Province: <BR>Country or Region: <BR>Postal Code: <BR>E-mail Address: fake.email@gmail.com</P><P>If you would like to change or add additional information to your customer profile please visit our Web site. While there you can take advantage of the many self-service features of our site including scheduling, appointment notifications, knowledge base look-up, and service request management. </P><P>We look forward to serving you in the future.</P>
     <P>Thank you.</P>";
        
        private const string preImageStoredAttributeName = "preimagename";
        private const string postImageStoredAttributeName = "postimagename";
        
        public SendTemplateTests()
        {
            _template = new Template()
            {
                Id = Guid.NewGuid(),
                TemplateTypeCode = Contact.EntityLogicalName,
                Subject = EMAIL_TEMPLATE_SUBJECT_XSLT,
                Body = EMAIL_TEMPLATE_BODY_XSLT,
                Title = EMAIL_TEMPLATE_TITLE
            };

            _contact = new Contact()
            {
                Id = Guid.NewGuid(),
                EMailAddress1 = DUMMY_EMAIL
            };

            _contact2 = new Contact()
            {
                Id = Guid.NewGuid(),
                EMailAddress1 = DUMMY_EMAIL_2
            };

            _account = new Account()
            {
                Id = Guid.NewGuid(),
                Name = ACCOUNT_NAME
            };
            
        }
        
        private EntityReference GetSender()
        {
            var systemUserResponse = (WhoAmIResponse)_service.Execute(new WhoAmIRequest());
            var userId = systemUserResponse.UserId;

            return new EntityReference(SystemUser.EntityLogicalName, userId);

        }
        
        
        [Theory]
        [InlineData(ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_trigger_send_template_plugin(ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<TracerPlugin>(new PluginStepDefinition()
            {
                MessageName = MessageNameConstants.Send,
                EntityLogicalName = Template.EntityLogicalName,
                Stage = stage,
                Mode = mode
            });

            _context.Initialize(new List<Entity>()
            {
                _template, _contact, _contact2, _account
            });
            
            var sender = GetSender();

            _request = new SendTemplateRequest()
            {
                Sender = sender,
                TemplateId = _template.Id,
                RegardingId = _account.Id,
                RegardingType = Account.EntityLogicalName,
                RecipientType = Contact.EntityLogicalName,
                RecipientIds = new[] { _contact.Id, _contact2.Id, }
            };
            
            var response = _service.Execute(_request);
            Assert.IsType<SendTemplateResponse>(response);
            
            var pluginStepAudit = _context.GetPluginStepAudit();
            var auditedSteps = pluginStepAudit.CreateQuery().ToList();

            Assert.Single(auditedSteps);

            var auditedStep = auditedSteps[0];
            Assert.Equal(MessageNameConstants.Send, auditedStep.MessageName);
            Assert.Equal(typeof(TracerPlugin), auditedStep.PluginAssemblyType);
            Assert.Equal(stage, auditedStep.Stage);
            Assert.Equal(mode, auditedStep.Mode);
            Assert.Equal(_template.Id, auditedStep.InputParameters["TemplateId"]);
        }
        
        [Theory]
        [InlineData(ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_trigger_update_email_plugin_twice_one_for_each_recipient(ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<TracerPlugin>(new PluginStepDefinition()
            {
                MessageName = MessageNameConstants.Update,
                EntityLogicalName = Email.EntityLogicalName,
                Stage = stage,
                Mode = mode
            });

            _context.Initialize(new List<Entity>()
            {
                _template, _contact, _contact2, _account
            });
            
            var sender = GetSender();
            _request = new SendTemplateRequest()
            {
                Sender = sender,
                TemplateId = _template.Id,
                RegardingId = _account.Id,
                RegardingType = Account.EntityLogicalName,
                RecipientType = Contact.EntityLogicalName,
                RecipientIds = new[] { _contact.Id, _contact2.Id, }
            };
            
            var response = _service.Execute(_request);
            Assert.IsType<SendTemplateResponse>(response);
            
            var pluginStepAudit = _context.GetPluginStepAudit();
            var auditedSteps = pluginStepAudit.CreateQuery().ToList();

            Assert.Equal(2, auditedSteps.Count);

            var auditedStep = auditedSteps[0];
            Assert.Equal(MessageNameConstants.Update, auditedStep.MessageName);
            Assert.Equal(typeof(TracerPlugin), auditedStep.PluginAssemblyType);
            Assert.Equal(stage, auditedStep.Stage);
            Assert.Equal(mode, auditedStep.Mode);
            
            auditedStep = auditedSteps[1];
            Assert.Equal(MessageNameConstants.Update, auditedStep.MessageName);
            Assert.Equal(typeof(TracerPlugin), auditedStep.PluginAssemblyType);
            Assert.Equal(stage, auditedStep.Stage);
            Assert.Equal(mode, auditedStep.Mode);
        }
        
       
    }
}
