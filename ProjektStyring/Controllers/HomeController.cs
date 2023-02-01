using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using ProjektStyring.Models;
using ProjektStyring.Services;
using RestSharp;

namespace ProjektStyring.Controllers
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class HomeController : Controller
    {
        private readonly IProjectsCosmosDbService _projectsCosmosDbService;

        public HomeController(IProjectsCosmosDbService projectsICosmosDbService)
        {
            _projectsCosmosDbService = projectsICosmosDbService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                string token = await HttpContext.GetTokenAsync("access_token");
                APITemp.RefreshDepartments(token);
                if (User.IsInRole(EasyHandler.Student))
                {
                    var projectList = await _projectsCosmosDbService.GetItemsAsync($"SELECT * FROM c WHERE c.isComplete = false AND c.isDeleted = false");
                    ProjectModel myProjectList;

                    if (projectList.Any(x => x.StudentIds.Any(s => s == User.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value)))
                    {
                        myProjectList = projectList.First(x => x.StudentIds.Any(s => s == User.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value));
                        return View(myProjectList);
                    }
                    else
                        return View();
                }
                else if (User.IsInRole(EasyHandler.Instructor) || User.IsInRole(EasyHandler.Admin))
                {
                    return Redirect("Project");
                }
                else if (User.IsInRole(EasyHandler.Teacher))
                {
                    return Redirect("Task");
                }
                else
                {
                    return View();
                }
            }
            ViewBag.Current = "Index";
            return View();
        }

        [Authorize]
        public IActionResult IndexLoggedIn()
        {
            return View();
        }
       
        [Authorize]
        public IActionResult Logout()
        {
            return SignOut("Cookies", "oidc");
        }
       
        public IActionResult KundeForside()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
