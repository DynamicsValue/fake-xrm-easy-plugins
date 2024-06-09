using System;
using FakeItEasy;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Pipeline.Scope
{
    internal static class PipelineOrganizationServiceFactory
    {
        /// <summary>
        /// Creates a new IPipelineOrganizationService from a previous IOrganizationService with a given scope
        /// </summary>
        /// <param name="service">An existing IOrganizationService that was created from an IXrmFakedContext</param>
        /// <param name="scope">The EventPipelineScope that will be stored in this service</param>
        /// <returns></returns>
        public static IPipelineOrganizationService New(IOrganizationService service, EventPipelineScope scope)
        {
            var pipelineOrganizationService = A.Fake<IPipelineOrganizationService>();
            pipelineOrganizationService.Scope = scope;
            AddCrudMethods(pipelineOrganizationService, service);
            return pipelineOrganizationService;
        }

        private static void AddCrudMethods(IPipelineOrganizationService pipelineService, IOrganizationService service)
        {
            AddFakeCreate(pipelineService, service);
            AddFakeUpdate(pipelineService, service);
            AddFakeDelete(pipelineService, service);
            AddFakeRetrieveMultiple(pipelineService, service);
            AddFakeRetrieve(pipelineService, service);
            AddFakeAssociate(pipelineService, service);
            AddFakeDisassociate(pipelineService, service);
            AddFakeExecute(pipelineService, service);
        }
        
        private static void AddFakeCreate(IPipelineOrganizationService pipelineService, IOrganizationService service) 
        {
            A.CallTo(() => pipelineService.Create(A<Entity>._))
                .ReturnsLazily((Entity e) =>
                {
                    var response = service.Execute(new PipelineOrganizationRequest()
                    {
                        OriginalRequest = new CreateRequest() { Target = e },
                        CurrentScope = pipelineService.Scope
                    });
                    return (response as CreateResponse).id;
                });
        }
        
        private static void AddFakeUpdate(IPipelineOrganizationService pipelineService, IOrganizationService service)
        {
            A.CallTo(() => pipelineService.Update(A<Entity>._))
                .Invokes((Entity e) =>
                {
                    service.Execute(new PipelineOrganizationRequest()
                    {
                        OriginalRequest = new UpdateRequest() { Target = e },
                        CurrentScope = pipelineService.Scope
                    });
                });
        }
        
        private static void AddFakeRetrieve(IPipelineOrganizationService pipelineService, IOrganizationService service)
        {
            A.CallTo(() => pipelineService.Retrieve(A<string>._, A<Guid>._, A<ColumnSet>._))
                .ReturnsLazily((string entityName, Guid id, ColumnSet columnSet) 
                    =>
                {
                    var response = service.Execute(new PipelineOrganizationRequest()
                    {
                        OriginalRequest = new RetrieveRequest()
                        {
                            Target = new EntityReference(entityName, id),
                            ColumnSet = columnSet
                        },
                        CurrentScope = pipelineService.Scope
                    });
                    
                    return (response as RetrieveResponse).Entity;
                });
        }
        
        private static void AddFakeRetrieveMultiple(IPipelineOrganizationService pipelineService, IOrganizationService service)
        {
            A.CallTo(() => pipelineService.RetrieveMultiple(A<QueryBase>._))
                .ReturnsLazily((QueryBase req) =>
                {
                    var response = service.Execute(new PipelineOrganizationRequest()
                    {
                        OriginalRequest = new RetrieveMultipleRequest()
                        {
                            Query = req
                        },
                        CurrentScope = pipelineService.Scope
                    });

                    return (response as RetrieveMultipleResponse).EntityCollection;
                });
        }
        
        private static void AddFakeDelete(IPipelineOrganizationService pipelineService, IOrganizationService service)
        {
            A.CallTo(() => pipelineService.Delete(A<string>._, A<Guid>._))
                .Invokes((string entityName, Guid id) =>
                {
                    service.Execute(new PipelineOrganizationRequest()
                    {
                        OriginalRequest = new DeleteRequest()
                        {
                            Target = new EntityReference(entityName, id)
                        },
                        CurrentScope = pipelineService.Scope
                    });
                });
        }
        
        private static void AddFakeAssociate(IPipelineOrganizationService pipelineService, IOrganizationService service)
        {
            A.CallTo(() =>
                    pipelineService.Associate(A<string>._, A<Guid>._, A<Relationship>._,
                        A<EntityReferenceCollection>._))
                .Invokes((string entityName, Guid entityId, Relationship relationship,
                    EntityReferenceCollection entityCollection) =>
                {
                    service.Execute(new PipelineOrganizationRequest()
                    {
                        OriginalRequest = new AssociateRequest()
                        {
                            Target = new EntityReference(entityName, entityId),
                            Relationship = relationship,
                            RelatedEntities = entityCollection
                        },
                        CurrentScope = pipelineService.Scope
                    });
                });
        }
        
        private static void AddFakeDisassociate(IPipelineOrganizationService pipelineService, IOrganizationService service)
        {
            A.CallTo(() =>
                    pipelineService.Disassociate(A<string>._, A<Guid>._, A<Relationship>._,
                        A<EntityReferenceCollection>._))
                .Invokes((string entityName, Guid entityId, Relationship relationship,
                    EntityReferenceCollection entityCollection) =>
                {
                    service.Execute(new PipelineOrganizationRequest()
                    {
                        OriginalRequest = new DisassociateRequest()
                        {
                            Target = new EntityReference(entityName, entityId),
                            Relationship = relationship,
                            RelatedEntities = entityCollection
                        },
                        CurrentScope = pipelineService.Scope
                    });
                });
        }
        
        private static void AddFakeExecute(IPipelineOrganizationService pipelineService, IOrganizationService service)
        {
            A.CallTo(() => pipelineService.Execute(A<OrganizationRequest>._))
                .ReturnsLazily((OrganizationRequest request) => 
                    service.Execute(new PipelineOrganizationRequest()
                    {
                        OriginalRequest = request,
                        CurrentScope = pipelineService.Scope
                    })
                );
        }
    }
}