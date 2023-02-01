using ProjektStyring.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace ProjektStyring.Services
{
    public class FeedbackCosmosDbService : IFeedbackCosmosDbService
    {
        private readonly Container _container;

        public FeedbackCosmosDbService(CosmosClient dbClient, string databaseName, string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task AddFeedbackAsync(FeedbackModel feedback)
        {
            await this._container.CreateItemAsync<FeedbackModel>(feedback, new PartitionKey(feedback.Id));
        }
    }
}
