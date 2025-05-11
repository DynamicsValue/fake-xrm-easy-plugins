using System;
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.Audit;
using FakeXrmEasy.Plugins.PluginImages;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.Tests.PluginsForTesting;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Xunit;
using DataverseEntities;

namespace FakeXrmEasy.Plugins.Tests.Pipeline.AssignMessageTests
{
    public class AssignMessageTests: FakeXrmEasyPipelineWithAuditAndMessagesTestsBase
    {
        private readonly SystemUser _user;
        private readonly Account _account;
        private readonly AssignRequest _request;
        
        private const string preImageStoredAttributeName = "preimagename";
        private const string postImageStoredAttributeName = "postimagename";
        
        public AssignMessageTests()
        {
            _user = new SystemUser()
            {
                Id = Guid.NewGuid()
            };
            
            _account = new Account()
            {
                Id = Guid.NewGuid(),
                Name = "Test account"
            };

            _request = new AssignRequest()
            {
                Assignee = _user.ToEntityReference(),
                Target = _account.ToEntityReference()
            };
        }

        [Theory]
        [InlineData(ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_trigger_assign_plugin(ProcessingStepStage stage, ProcessingStepMode mode)
        {
            _context.RegisterPluginStep<TracerPlugin>(new PluginStepDefinition()
            {
                MessageName = MessageNameConstants.Assign,
                EntityLogicalName = Account.EntityLogicalName,
                Stage = stage,
                Mode = mode
            });

            _context.Initialize(new List<Entity>()
            {
                _account, _user
            });
            
            var response = _service.Execute(_request);
            Assert.IsType<AssignResponse>(response);
            
            var pluginStepAudit = _context.GetPluginStepAudit();
            var auditedSteps = pluginStepAudit.CreateQuery().ToList();

            Assert.Single(auditedSteps);

            var auditedStep = auditedSteps[0];
            Assert.Equal(MessageNameConstants.Assign, auditedStep.MessageName);
            Assert.Equal(typeof(TracerPlugin), auditedStep.PluginAssemblyType);
            Assert.Equal(stage, auditedStep.Stage);
            Assert.Equal(mode, auditedStep.Mode);
        }
        
        [Theory]
        [InlineData(ProcessingStepStage.Prevalidation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Preoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Synchronous)]
        [InlineData(ProcessingStepStage.Postoperation, ProcessingStepMode.Asynchronous)]
        public void Should_pass_preimage_when_there_is_a_registered_preimage(ProcessingStepStage stage, ProcessingStepMode mode)
        {
            string registeredPreImageName = "PreImage";
            PluginImageDefinition preImageDefinition = new PluginImageDefinition(registeredPreImageName, ProcessingStepImageType.PreImage);

            _context.RegisterPluginStep<TracerPlugin>(new PluginStepDefinition()
            {
                MessageName = MessageNameConstants.Assign,
                EntityLogicalName = Account.EntityLogicalName,
                Stage = stage,
                Mode = mode,
                ImagesDefinitions = new List<PluginImageDefinition>()
                {
                    preImageDefinition
                }
            });

            _context.Initialize(new List<Entity>()
            {
                _account, _user
            });
            
            //Act
            var response = _service.Execute(_request);
            Assert.IsType<AssignResponse>(response);

            //Assert
            var pluginStepAudit = _context.GetPluginStepAudit();
            var auditedSteps = pluginStepAudit.CreateQuery().ToList();

            Assert.Single(auditedSteps);
            var auditedStep = auditedSteps[0];

            var preImage = auditedStep.PluginContext.PreEntityImages[registeredPreImageName];
            Assert.NotNull(preImage);
            Assert.Equal(_account.Id, preImage.Id);
            Assert.Equal(Account.EntityLogicalName, preImage.LogicalName);
        }
        
        [Fact]
        public void Should_pass_post_image_when_there_is_a_registered_post_image()
        {
            string imageName = "PostImage";
            PluginImageDefinition imageDefinition = new PluginImageDefinition(imageName, ProcessingStepImageType.PostImage);

            _context.RegisterPluginStep<EntityImagesInPluginPipeline>(new PluginStepDefinition()
            {
                MessageName = MessageNameConstants.Assign,
                ImagesDefinitions = new List<PluginImageDefinition>()
                {
                    imageDefinition
                }
            });

            _context.Initialize(new List<Entity>()
            {
                _account, _user
            });
            
            //Act
            var response = _service.Execute(_request);
            Assert.IsType<AssignResponse>(response);
            

            //Assert
            var allAccounts = _context.CreateQuery<Account>().ToList();

            var updatedAccount = allAccounts.Where(a => a.Id == _account.Id);
            Assert.NotNull(updatedAccount);

            var preImages = allAccounts.Where(a => a.Contains(preImageStoredAttributeName)).ToList();
            var postImages = allAccounts.Where(a => a.Contains(postImageStoredAttributeName)).ToList();

            Assert.Empty(preImages);
            Assert.Single(postImages);

            var postImage = postImages.First();
            Assert.Equal(imageName, postImage.GetAttributeValue<string>("postimagename"));
        }
    }
}