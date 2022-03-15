using Crm;
using FakeXrmEasy.Abstractions.Plugins.Enums;
using FakeXrmEasy.Pipeline;
using FakeXrmEasy.Plugins.Tests.PluginsForTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline
{
    public class PipelinePluginImagesTests: FakeXrmEasyPipelineTests
    {
        private readonly Contact _previousContact;
        private readonly Contact _newContact;
        private readonly Account _account;
        private readonly Account _target;

        private const string preImageStoredAttributeName = "preimagename";
        private const string postImageStoredAttributeName = "postimagename";

        public PipelinePluginImagesTests()
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
                AccountNumber = "1234",
                AccountCategoryCode = new OptionSetValue(2),
                NumberOfEmployees = 10,
                Revenue = new Money(10000),
                PrimaryContactId = _newContact.ToEntityReference()
            };
        }

        [Fact]
        public void Should_pass_preimage_when_there_is_a_registered_preimage()
        {
            _context.Initialize(new List<Entity>()
            {
                _newContact, _previousContact, _account
            });

            string registeredPreImageName = "PreImage";
            PluginImageDefinition preImageDefinition = new PluginImageDefinition(registeredPreImageName, ProcessingStepImageType.PreImage);

            _context.RegisterPluginStep<EntityImagesInPluginPipeline>("Update", registeredImages: new PluginImageDefinition[] { preImageDefinition });

            // act
            _service.Update(_target);

            var allAccounts = _context.CreateQuery<Account>().ToList();

            var updatedAccount = allAccounts.Where(a => a.Id == _account.Id);
            Assert.NotNull(updatedAccount);

            var preImages = allAccounts.Where(a => a.Contains(preImageStoredAttributeName)).ToList();
            var postImages = allAccounts.Where(a => a.Contains(postImageStoredAttributeName)).ToList();

            Assert.Single(preImages);
            Assert.Empty(postImages);

            var preImage = preImages.First();
            Assert.Equal(registeredPreImageName, preImage.GetAttributeValue<string>("preimagename"));
        }
    }
}
