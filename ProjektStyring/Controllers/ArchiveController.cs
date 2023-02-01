using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjektStyring.Models;
using ProjektStyring.Services;

namespace ProjektStyring.Controllers
{
    [Authorize(Roles = "Instructor,Administrator")]
    public class ArchiveController : Controller
    {
        private readonly IProjectsCosmosDbService _projectsCosmosDbService;
        public ArchiveController(IProjectsCosmosDbService projectsCosmosDbService)
        {
            _projectsCosmosDbService = projectsCosmosDbService;
        }

        [ActionName("Index")]
        public async Task<IActionResult> Index()
        {
            string token = await HttpContext.GetTokenAsync("access_token");
            APITemp.RefreshUserList(token);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(await HttpContext.GetTokenAsync("access_token")) as JwtSecurityToken;
            var departments = jwtToken.Claims.Where(claim => claim.Type == "department").ToList();
            Dictionary<string, string> myDepartments = new Dictionary<string, string>();
            foreach (var item in departments)
            {
                myDepartments.Add(item.Value.Split(":")[0], item.Value.Split(":")[1]);
            }
            var projectList = await _projectsCosmosDbService.GetItemsAsync("SELECT * FROM c WHERE c.isComplete = true");
            if (User.IsInRole(EasyHandler.Admin))
            {
                return View(projectList);
            }
            var myProjectList = projectList.Where(x => myDepartments.Keys.Contains(x.DepartmentId));
            ViewBag.Current = "Arkiv";
            return View(myProjectList);

        }
        [ActionName("Details")]
        public async Task<IActionResult> DetailsAsync(string id)
        {
            string token = await HttpContext.GetTokenAsync("access_token");
            APITemp.RefreshUserList(token);
            ViewBag.Current = "Arkiv";
            return View(await _projectsCosmosDbService.GetItemAsync(id));
        }

        [ActionName("DeleteAsAdmin")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            ViewBag.Current = "Arkiv";
            if (id == null || id.Length < 1)
            {
                return BadRequest();
            }

            ProjectModel project = await _projectsCosmosDbService.GetItemAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            await _projectsCosmosDbService.DeleteItemAsync(id);
            return Redirect("/Project/Index");
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedAsync([Bind("Id")] string id)
        {
            await _projectsCosmosDbService.DeleteItemAsync(id);
            return RedirectToAction("/Project/Index");
        }

        [ActionName("ShowDeleted")]
        public async Task<IActionResult> ShowDeletedAsync()
        {
            ViewBag.Current = "Arkiv";
            ViewBag.ShowingDeleted = "1";
            string token = await HttpContext.GetTokenAsync("access_token");
            APITemp.RefreshUserList(token);
            var projectList = await _projectsCosmosDbService.GetItemsAsync("SELECT * FROM c WHERE c.isComplete = true or c.isDeleted = true");
            return View("Index", projectList);
        }

        [ActionName("HideDeleted")]
        public async Task<IActionResult> HideDeletedAsync()
        {
            ViewBag.Current = "Arkiv";
            ViewBag.ShowingDeleted = "0";
            string token = await HttpContext.GetTokenAsync("access_token");
            APITemp.RefreshUserList(token);
            var projectList = await _projectsCosmosDbService.GetItemsAsync("SELECT * FROM c WHERE c.isComplete = true");
            return View("Index", projectList);
        }
    }
}
