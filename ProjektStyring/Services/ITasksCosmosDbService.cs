namespace ProjektStyring.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ProjektStyring.Models;

    public interface ITasksCosmosDbService
    {
        Task<IEnumerable<TaskModel>> GetItemsAsync(string query);
        Task<TaskModel> GetItemAsync(string id);
        Task AddItemAsync(TaskModel task);
        Task UpdateItemAsync(string id, TaskModel task);
        Task DeleteItemAsync(string id);
    }
}