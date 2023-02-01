namespace ProjektStyring.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ProjektStyring.Models;

    public interface IProjectsCosmosDbService
    {
        Task<IEnumerable<ProjectModel>> GetItemsAsync(string query); 
        Task<ProjectModel> GetItemAsync(string id);
        Task AddItemAsync(ProjectModel project);
        Task UpdateItemAsync(string id, ProjectModel project);
        Task DeleteItemAsync(string id);
    }
}