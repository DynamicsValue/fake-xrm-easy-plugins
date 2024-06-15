using System;
using System.Collections.Generic;
using DataverseEntities;
using FakeXrmEasy.Pipeline;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Xunit;

namespace FakeXrmEasy.Plugins.Tests.Pipeline.PipelineProcessorTests
{
    public class GetPostEntityImageTests: FakeXrmEasyTestsBase
    {
        private readonly Account _account1;
        private readonly Account _account2;

        private const string _postValue = "69";
        
        public GetPostEntityImageTests()
        {
            _account1 = new Account()
            {
                Id = Guid.NewGuid(),
                Name = "Account 1",
                Telephone1 = "1"
            };
            
            _account2 = new Account()
            {
                Id = Guid.NewGuid(),
                Name = "Account 2",
                Telephone1 = "2"
            };
        }

        [Fact]
        public void Should_return_post_entity_image()
        {
            _context.Initialize(_account1);

            var request = new UpdateRequest()
            {
                Target = new Account()
                {
                    Id = _account1.Id,
                    Telephone1 = _postValue
                }
            };

            var postEntityImage = PipelineProcessor.GetPostImageEntityForRequest(_context, request);

            AssertPostEntityImage(_account1, postEntityImage);
        }
        
#if FAKE_XRM_EASY_9
        [Fact]
        public void Should_return_post_entity_image_collection()
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
                        Telephone1 = _postValue
                    },
                    new Account()
                    {
                        Id = _account2.Id,
                        Telephone1 = _postValue
                    }
                })
                {
                    EntityName = Account.EntityLogicalName
                }
            };

            var postEntityImageCollection = PipelineProcessor.GetPostImageEntityCollectionForRequest(_context, request);
            Assert.NotNull(postEntityImageCollection);

            var postImageEntities = postEntityImageCollection.Entities;
            AssertPostEntityImage(_account1, postImageEntities[0]);
            AssertPostEntityImage(_account2, postImageEntities[1]);

        }
#endif
        
        private void AssertPostEntityImage(Account expected, Entity postImage)
        {
            Assert.Equal(expected.Id, postImage.Id);
            Assert.Equal(expected.Name, postImage["name"]);
            Assert.True(postImage.Attributes.ContainsKey("telephone1"));
            Assert.Equal(_postValue, postImage["telephone1"]);
        }
    }
}