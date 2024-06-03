using Crm;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.PluginImages;
using FakeXrmEasy.Plugins.PluginSteps;
using FakeXrmEasy.Plugins.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline
{
    public class PipelinePluginPostImagesTests: FakeXrmEasyPipelineTestsBase
    {
        private readonly Contact _previousContact;
        private readonly Contact _newContact;
        private readonly Account _account;
        private readonly Account _target;

        private const string preImageStoredAttributeName = "preimagename";
        private const string postImageStoredAttributeName = "postimagename";

        public PipelinePluginPostImagesTests()
        {
            _previousContact = new Contact()
            {
                Id = Guid.NewGuid(),
                LastName = "Previous"
            };

            _newContact = new Contact()
            {
                Id = Guid.NewGuid(),
                LastName = "New"
            };

            _account = new Account()
            {
                Id = Guid.NewGuid(),
                AccountNumber = "1234567890",
                AccountCategoryCode = new OptionSetValue(1),
                NumberOfEmployees = 5,
                Revenue = new Money(20000),
                PrimaryContactId = _previousContact.ToEntityReference(),
                Telephone1 = "+123456"
            };

            _target = new Account()
            {
                AccountId = _account.Id,
                AccountNumber = "1234"
            };
        }

        [Fact]
        public void Should_pass_post_image_when_there_is_a_registered_post_image()
        {
            _context.Initialize(new List<Entity>()
            {
                _newContact, _previousContact, _account
            });

            string imageName = "PostImage";
            PluginImageDefinition imageDefinition = new PluginImageDefinition(imageName, ProcessingStepImageType.PostImage);

            _context.RegisterPluginStep<EntityImagesInPluginPipeline>(new PluginStepDefinition()
            {
                MessageName = "Update",
                ImagesDefinitions = new List<PluginImageDefinition>()
                {
                    imageDefinition
                }
            });

            //Act
            _service.Update(_target);

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

        [Fact]
        public void Should_pass_post_image_attributes_along_with_target_when_there_is_a_registered_post_image()
        {
            _context.Initialize(new List<Entity>()
            {
                _newContact, _previousContact, _account
            });

            string imageName = "PostImage";
            PluginImageDefinition imageDefinition = new PluginImageDefinition(
                imageName, 
                ProcessingStepImageType.PostImage, 
                new string[] { "accountnumber", "numberofemployees" });

            _context.RegisterPluginStep<EntityImagesInPluginPipeline>(new PluginStepDefinition()
            {
                MessageName = "Update",
                ImagesDefinitions = new List<PluginImageDefinition>()
                {
                    imageDefinition
                }
            });

            //Act
            _service.Update(_target);

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
            Assert.Equal(_target.AccountNumber, postImage.GetAttributeValue<string>("accountnumber"));
            Assert.Equal(_account.NumberOfEmployees, postImage.GetAttributeValue<int?>("numberofemployees"));
            
        }

        
    }
}
