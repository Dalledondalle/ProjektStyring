using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjektStyring.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;


namespace ProjektStyring.Services
{
    public class ProjectsCosmosDbService : IProjectsCosmosDbService
    {
        private Container _container;

        public ProjectsCosmosDbService(
            CosmosClient dbClient,
            string databaseName,

            string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task AddItemAsync(ProjectModel project)
        {
            await this._container.CreateItemAsync<ProjectModel>(project, new PartitionKey(project.Id));
        }

        public async Task DeleteItemAsync(string id)
        {
            await this._container.DeleteItemAsync<ProjectModel>(id, new PartitionKey(id));
        }

        public async Task<ProjectModel> GetItemAsync(string id)
        {
            try
            {
                ItemResponse<ProjectModel> response = await this._container.ReadItemAsync<ProjectModel>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<IEnumerable<ProjectModel>> GetItemsAsync(string queryString)
        {
            var query = this._container.GetItemQueryIterator<ProjectModel>(new QueryDefinition(queryString));
            List<ProjectModel> results = new List<ProjectModel>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task UpdateItemAsync(string id, ProjectModel project)
        {
            await this._container.UpsertItemAsync<ProjectModel>(project, new PartitionKey(id));
        }
    }
}
