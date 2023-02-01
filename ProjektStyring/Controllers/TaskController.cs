using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjektStyring.Models;
using Microsoft.AspNetCore.Authorization;
using ProjektStyring.Services;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;

namespace ProjektStyring.Controllers
{
    [Authorize(Roles = "Instructor,Administrator,StockManager")]
    public class TaskController : Controller
    {
        private readonly ITasksCosmosDbService _tasksCosmosDbService;
        private readonly IDocumentationBlobService _documentationBlobService;

        public TaskController(ITasksCosmosDbService tasksCosmosDbService, IDocumentationBlobService documentationBlobService)
        {
            _tasksCosmosDbService = tasksCosmosDbService;
            _documentationBlobService = documentationBlobService;
        }

        [ActionName("Index")]
        public async Task<IActionResult> Index()
        {
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            string userId = User.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            IEnumerable<TaskModel> tasklist;
            if (User.IsInRole(EasyHandler.Admin) || User.IsInRole(EasyHandler.Instructor))
                tasklist = await _tasksCosmosDbService.GetItemsAsync($"SELECT * FROM c WHERE c.isDeleted = false");
            else
                tasklist = await _tasksCosmosDbService.GetItemsAsync($"SELECT * FROM c WHERE c.isDeleted = false and c.customerId = '{userId}'");
            ViewBag.Current = "Opgaver";
            return View(tasklist);
        }

        [ActionName("Details")]
        public async Task<IActionResult> DetailsAsync(string id)
        {
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            ViewBag.Current = "Opgaver";
            TaskModel task = await _tasksCosmosDbService.GetItemAsync(id);
            if (User.IsInRole(EasyHandler.Teacher) && task.CustomerId != User.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value)
                return Redirect("Index");
            return View(task);
        }

        [ActionName("Create")]
        public async Task<IActionResult> Create()
        {
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            APITemp.RefreshInstructorList(await HttpContext.GetTokenAsync("access_token"));
            APITemp.RefreshStudentList(await HttpContext.GetTokenAsync("access_token"));
            APITemp.RefreshCustomerList(await HttpContext.GetTokenAsync("access_token"));
            ViewBag.Current = "Opgaver";
            return View();
        }

        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(TaskModel task, List<IFormFile> files)
        {
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            ViewBag.Current = "Opgaver";
            if (ModelState.IsValid)
            {
                try
                {
                    task.Id = Guid.NewGuid().ToString();
                    task.CustomerId = User.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                    task.Blobs = new List<Blob>();
                    await _documentationBlobService.CreateContainerAsync(new ProjectModel() { Id = task.Id });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    ViewBag.Message = String.Format("Fejl i formularen - Tjek venligst felterne efter igen");
                    return View();
                }
                finally
                {
                    foreach (FormFile f in files)
                    {
                        Debug.WriteLine(f.Name);
                        Blob b = new Blob();
                        b.Name = f.Name;
                        b.Url = _documentationBlobService.UploadDocumentationAsFileAsync(new ProjectModel() { Id = task.Id }, f).Result.ToString();
                        task.Blobs.Add(b);
                    }
                    task.CreatedTime = DateTime.Now.ToUniversalTime();
                    await _tasksCosmosDbService.AddItemAsync(task);
                }
            }
            return RedirectToAction("Index");
        }


        [ActionName("Delete")]
        [Authorize(Roles = "Instructor,Administrator")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            ViewBag.Current = "Opgaver";
            if (id == null || id.Length < 1)
            {
                return BadRequest();
            }

            TaskModel task = await _tasksCosmosDbService.GetItemAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            task.IsDeleted = true;
            await _tasksCosmosDbService.UpdateItemAsync(id, task);

            return Redirect("/Task/Index");
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor,Administrator")]
        public async Task<IActionResult> DeleteConfirmedAsync([Bind("CaseId")] string id)
        {
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            ViewBag.Current = "Opgaver";
            await _tasksCosmosDbService.DeleteItemAsync(id);
            return RedirectToAction("/Task/Index");
        }

        [ActionName("Edit")]
        public async Task<IActionResult> EditAsync(string id)
        {
            ViewBag.Current = "Opgaver";
            if (id == null)
            {
                return BadRequest();
            }
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            APITemp.RefreshInstructorList(await HttpContext.GetTokenAsync("access_token"));
            APITemp.RefreshStudentList(await HttpContext.GetTokenAsync("access_token"));
            APITemp.RefreshCustomerList(await HttpContext.GetTokenAsync("access_token"));
            TaskModel task = await _tasksCosmosDbService.GetItemAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            return View(task);
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsync(TaskModel task)
        {
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            ViewBag.Current = "Opgaver";
            if (ModelState.IsValid)
            {
                await _tasksCosmosDbService.UpdateItemAsync(task.Id, task);
                return RedirectToAction("Index");
            }

            return View(task);
        }

        [ActionName("AcceptTask")]
        public async Task<IActionResult> AcceptTask(string taskId)
        {
            TaskModel task = await _tasksCosmosDbService.GetItemAsync(taskId);
            task.CurrentStatus = CurrentStatus.Accepteret;
            await _tasksCosmosDbService.UpdateItemAsync(taskId, task);
            return Redirect($"/Task/Details/{taskId}");
        }

        [ActionName("DeclineTask")]
        public async Task<IActionResult> DeclineTask(string taskId)
        {
            TaskModel task = await _tasksCosmosDbService.GetItemAsync(taskId);
            task.CurrentStatus = CurrentStatus.Afvist;
            await _tasksCosmosDbService.UpdateItemAsync(taskId, task);
            return Redirect($"/Task/Details/{taskId}");
        }
    }
}
