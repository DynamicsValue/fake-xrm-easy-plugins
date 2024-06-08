using System;
using FakeItEasy;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Pipeline.Scope
{
    internal class PipelineOrganizationServiceFactory
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
        }
        
        private static void AddFakeCreate(IPipelineOrganizationService pipelineService, IOrganizationService service) 
        {
            A.CallTo(() => pipelineService.Create(A<Entity>._))
                .ReturnsLazily((Entity e) =>
                    service.Create(e));
        }
        
        private static void AddFakeUpdate(IPipelineOrganizationService pipelineService, IOrganizationService service) 
        {
            A.CallTo(() => pipelineService.Update(A<Entity>._))
                .Invokes((Entity e) =>
                    service.Update(e));
        }
        
        private static void AddFakeRetrieve(IPipelineOrganizationService pipelineService, IOrganizationService service)
        {
            A.CallTo(() => pipelineService.Retrieve(A<string>._, A<Guid>._, A<ColumnSet>._))
                .ReturnsLazily((string entityName, Guid id, ColumnSet columnSet) 
                    => service.Retrieve(entityName, id, columnSet));
        }
        
        private static void AddFakeRetrieveMultiple(IPipelineOrganizationService pipelineService, IOrganizationService service)
        {
            A.CallTo(() => pipelineService.RetrieveMultiple(A<QueryBase>._))
                .ReturnsLazily((QueryBase req) =>
                    service.RetrieveMultiple(req));
        }
        
        private static void AddFakeDelete(IPipelineOrganizationService pipelineService, IOrganizationService service)
        {
            A.CallTo(() => pipelineService.Delete(A<string>._, A<Guid>._))
                .Invokes((string entityName, Guid id) =>
                    service.Delete(entityName, id));
        }
        
        private static void AddFakeAssociate(IPipelineOrganizationService pipelineService, IOrganizationService service)
        {
            A.CallTo(() => pipelineService.Associate(A<string>._, A<Guid>._, A<Relationship>._, A<EntityReferenceCollection>._))
                .Invokes((string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection entityCollection) =>
                    service.Associate(entityName, entityId, relationship, entityCollection));
        }
        
        private static void AddFakeDisassociate(IPipelineOrganizationService pipelineService, IOrganizationService service)
        {
            A.CallTo(() => pipelineService.Disassociate(A<string>._, A<Guid>._, A<Relationship>._, A<EntityReferenceCollection>._))
                .Invokes((string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection entityCollection) =>
                    service.Disassociate(entityName, entityId, relationship, entityCollection));
        }
    }
}