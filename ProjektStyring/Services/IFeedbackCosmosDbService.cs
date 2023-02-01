using ProjektStyring.Models;
using System.Threading.Tasks;

namespace ProjektStyring.Services
{
    public interface IFeedbackCosmosDbService
    {
        Task AddFeedbackAsync(FeedbackModel feedback);
    }
}