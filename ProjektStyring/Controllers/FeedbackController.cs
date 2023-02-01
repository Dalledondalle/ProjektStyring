using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjektStyring.Models;
using ProjektStyring.Services;

namespace ProjektStyring.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly IFeedbackCosmosDbService _feedbackCosmosDbService;

        public FeedbackController(IFeedbackCosmosDbService feedbackCosmosDbService)
        {
            _feedbackCosmosDbService = feedbackCosmosDbService;
        }

        public IActionResult Index()
        {            
            ViewBag.Current = "Feedback";
            return View();
        }

        [HttpPost]
        [ActionName("PostFeedback")]
        public async Task<IActionResult> PostFeedback(string content)
        {
            FeedbackModel feedback = new FeedbackModel();
            if (User.Identity.IsAuthenticated)
                feedback.UserId = User.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            else
                feedback.UserId = "Not logged in";
            feedback.PostedDate = DateTime.UtcNow;
            feedback.Id = Guid.NewGuid().ToString();
            feedback.Content = content;
            await _feedbackCosmosDbService.AddFeedbackAsync(feedback);
            TempData["msg"] = "<script>alert('Tak for din feedback!');</script>";
            return Redirect("Index");
        }
    }    
}
