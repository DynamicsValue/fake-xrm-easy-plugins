using System;
using System.Collections.Generic;
using DataverseEntities;
using FakeXrmEasy.Pipeline;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline.PipelineProcessorTests
{
    public class GetPreEntityImageTests: FakeXrmEasyTestsBase

    {
        private readonly Account _account1;
        private readonly Account _account2;
        
        public GetPreEntityImageTests()
        {
            _account1 = new Account()
            {
                Id = Guid.NewGuid(),
                Name = "Account 1"
            };
            
            _account2 = new Account()
            {
                Id = Guid.NewGuid(),
                Name = "Account 2"
            };
        }

        [Fact]
        public void Should_return_pre_entity_image()
        {
            _context.Initialize(_account1);

            var request = new UpdateRequest()
            {
                Target = new Account()
                {
                    Id = _account1.Id,
                    Telephone1 = "69 69"
                }
            };

            var preEntityImage = PipelineProcessor.GetPreImageEntityForRequest(_context, request);

            AssertPreEntityImage(_account1, preEntityImage);
        }
        
#if FAKE_XRM_EASY_9
        [Fact]
        public void Should_return_pre_entity_image_collection()
        {
            _context.Initialize(new List<Entity>()
            {
                _account1, _account2
            });

            var request = new UpdateMultipleRequest()
            {
                Targets = new EntityCollection(new List<Entity>()
                {
                    new Account()
                    {
                        Id = _account1.Id,
                        Telephone1 = "69 69"
                    },
                    new Account()
                    {
                        Id = _account2.Id,
                        Telephone1 = "79 79"
                    }
                })
                {
                    EntityName = Account.EntityLogicalName
                }
            };

            var preEntityImageCollection = PipelineProcessor.GetPreImageEntityCollectionForRequest(_context, request);
            Assert.NotNull(preEntityImageCollection);

            var preImageEntities = preEntityImageCollection.Entities;
            AssertPreEntityImage(_account1, preImageEntities[0]);
            AssertPreEntityImage(_account2, preImageEntities[1]);

        }
#endif
        
        private void AssertPreEntityImage(Account expected, Entity preImage)
        {
            Assert.Equal(expected.Id, preImage.Id);
            Assert.Equal(expected.Name, preImage["name"]);
            Assert.False(preImage.Attributes.ContainsKey("telephone1"));
        }
    }
}