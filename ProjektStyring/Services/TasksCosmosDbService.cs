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
    public class TasksCosmosDbService : ITasksCosmosDbService
    {
        private Container _container;

        public TasksCosmosDbService(
            CosmosClient dbClient,
            string databaseName,

            string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }


        public async Task AddItemAsync(TaskModel task)
        {
            await this._container.CreateItemAsync<TaskModel>(task, new PartitionKey(task.Id));
        }

        public async Task DeleteItemAsync(string id)
        {
            await this._container.DeleteItemAsync<ProjectModel>(id, new PartitionKey(id));
        }

        public async Task<TaskModel> GetItemAsync(string id)
        {
            try
            {
                ItemResponse<TaskModel> response = await this._container.ReadItemAsync<TaskModel>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

        }

        public async Task<IEnumerable<TaskModel>> GetItemsAsync(string queryString)
        {
            var query = this._container.GetItemQueryIterator<TaskModel>(new QueryDefinition(queryString));
            List<TaskModel> results = new List<TaskModel>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task UpdateItemAsync(string id, TaskModel task)
        {
            await this._container.UpsertItemAsync<TaskModel>(task, new PartitionKey(id));
        }
    }
}
