using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjektStyring.Models;
using ProjektStyring.Services;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Web;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;

namespace ProjektStyring.Controllers
{
    [Authorize(Roles = "Student,Instructor,Administrator")]
    public class ProjectController : Controller
    {
        private readonly IProjectsCosmosDbService _projectsCosmosDbService;
        private readonly IDocumentationBlobService _documentationBlobService;
        private readonly ITasksCosmosDbService _tasksCosmosDbService;

        public ProjectController(IProjectsCosmosDbService projectsCosmosDbService, IDocumentationBlobService documentationBlobService, ITasksCosmosDbService tasksCosmosDbService)
        {
            _projectsCosmosDbService = projectsCosmosDbService;
            _documentationBlobService = documentationBlobService;
            _tasksCosmosDbService = tasksCosmosDbService;
        }

        [ActionName("Index")]
        public async Task<IActionResult> Index()
        {
            string token = await HttpContext.GetTokenAsync("access_token");
            APITemp.RefreshUserList(token);
            var projectList = await _projectsCosmosDbService.GetItemsAsync("SELECT * FROM c WHERE c.isComplete = false AND c.isDeleted = false");
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(await HttpContext.GetTokenAsync("access_token")) as JwtSecurityToken;
            var departments = jwtToken.Claims.Where(claim => claim.Type == "department").ToList();
            Dictionary<string, string> myDepartments = new Dictionary<string, string>();
            foreach (var item in departments)
            {
                myDepartments.Add(item.Value.Split(":")[0], item.Value.Split(":")[1]);
            }
            if (User.IsInRole(EasyHandler.Student) && projectList.Any(x => x.StudentIds.Any(s => s == User.Claims.First(y => y.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value)))
            {
                ProjectModel project = projectList.First(x => x.StudentIds.Any(s => s == User.Claims.First(y => y.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value));
                return Redirect($"Project/Details/{project.Id}");
            }
            APITemp.RefreshCustomerList(token);
            List<ProjectModel> myProject = projectList.Where(x => myDepartments.Keys.Contains(x.DepartmentId)).ToList();
            ViewBag.Current = "Projekter";
            return View(myProject);
        }
        [ActionName("Details")]
        public async Task<IActionResult> DetailsAsync(string id)
        {
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            ProjectModel project = await _projectsCosmosDbService.GetItemAsync(id);
            if (project.IsDeleted || project.IsComplete)
            {
                ViewBag.Current = "Arkiv";
            }
            else
            {
                ViewBag.Current = "Projekter";
            }
            return View(project);
        }

        [ActionName("Create")]
        [Authorize(Roles = "Instructor,Administrator,Student")]
        public async Task<IActionResult> Create()
        {
            string token = await HttpContext.GetTokenAsync("access_token");
            APITemp.RefreshDepartments(token);
            APITemp.RefreshInstructorList(token);
            APITemp.RefreshStudentList(token);
            APITemp.RefreshCustomerList(token);
            ViewBag.Current = "Projekter";
            return View();
        }

        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor,Administrator,Student")]
        public async Task<IActionResult> CreateAsync(ProjectModel project)
        {
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            ViewBag.Current = "Projekter";
            if (ModelState.IsValid)
            {
                try
                {
                    if (project.Id == null || project.Id.Length == 0)
                        project.Id = Guid.NewGuid().ToString();
                    if (project.CaseId == null || project.CaseId.Length == 0)
                    {
                        string[] ids = project.Id.Split('-');
                        project.CaseId = ids[1];
                    }
                    project.DocumentationList = new List<DescriptiveItem>();
                    project.Materials = new List<MaterialModel>();
                    project.History = new List<HistoryEntry>();
                    if (project.StudentIds == null)
                        project.StudentIds = new List<string>();
                    else
                    {
                        foreach (string s in project.StudentIds)
                        {
                            project.History.Add(new HistoryEntry()
                            {
                                StudentId = s,
                                LoginTime = DateTime.Now.ToUniversalTime()
                            });
                        }
                    }
                    project.IsComplete = false;
                    project.IsDeleted = false;
                    await _projectsCosmosDbService.AddItemAsync(project);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    if (_projectsCosmosDbService.GetItemsAsync($"SELECT * FROM c where c.id = '{project.Id}'").Result.Count() > 0) ViewBag.Message = String.Format("Sagsnummeret findes allerede");
                    else ViewBag.Message = String.Format("Fejl i formularen - Tjek venligst felterne efter igen");
                    Console.WriteLine(ex);
                    return View();
                }
            }
            return View(project);
        }

        [ActionName("AddDocumentation")]
        public async Task<IActionResult> AddDocumentationAsync(string id)
        {
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            ViewBag.Current = "Projekter";
            if (id == null || id.Length < 1)
            {
                return BadRequest();
            }

            ProjectModel project = await _projectsCosmosDbService.GetItemAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            return View();
        }

        [HttpPost]
        [ActionName("AddDocumentation")]
        public async Task<IActionResult> AddDocumentationAsync(DescriptiveItem descriptive, string id, List<IFormFile> files)
        {
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            ProjectModel project = await _projectsCosmosDbService.GetItemAsync(id);
            descriptive.StudentId = User.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
            descriptive.StartDate = DateTime.Now.ToUniversalTime();
            if (project.DocumentationList != null && project.DocumentationList.Count > 0)
                descriptive.Id = (project.DocumentationList.Max(x => int.Parse(x.Id)) + 1).ToString();
            else
                descriptive.Id = "1";
            if (project.DocumentationList == null) project.DocumentationList = new List<DescriptiveItem>();
            project.DocumentationList.Add(descriptive);
            try
            {
                await _documentationBlobService.CreateContainerAsync(project);
            }
            finally
            {
                descriptive.Blobs = new List<Blob>();
                if (ModelState.IsValid)
                {
                    foreach (FormFile f in files)
                    {
                        Blob b = new Blob();
                        b.Name = f.Name;
                        b.Url = _documentationBlobService.UploadDocumentationAsFileAsync(project, f).Result.ToString();
                        descriptive.Blobs.Add(b);
                    }
                    await _projectsCosmosDbService.UpdateItemAsync(id, project);
                }
            }
            return Redirect($"/Project/Details/{id}");
        }

        [ActionName("DeleteDocumentation")]
        [Authorize(Roles = "Instructor,Administrator")]
        public async Task<IActionResult> DeleteDocumentationAsync(string id, string descriptiveId)
        {
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            ViewBag.Current = "Projekter";
            if (id == null || id.Length < 1)
            {
                return BadRequest();
            }

            ProjectModel project = await _projectsCosmosDbService.GetItemAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            project.DocumentationList.First(x => x.Id == descriptiveId).IsDeleted = true;
            await _projectsCosmosDbService.UpdateItemAsync(project.Id, project);
            return Redirect($"/Project/Details/{id}");
        }

        [ActionName("Edit")]
        [Authorize(Roles = "Instructor,Administrator")]
        public async Task<IActionResult> EditAsync(string id)
        {
            string token = await HttpContext.GetTokenAsync("access_token");
            APITemp.RefreshDepartments(token);
            APITemp.RefreshInstructorList(token);
            APITemp.RefreshStudentList(token);
            APITemp.RefreshCustomerList(token);
            ViewBag.Current = "Projekter";
            if (id == null)
            {
                return BadRequest();
            }
            ProjectModel project = await _projectsCosmosDbService.GetItemAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor,Administrator")]
        public async Task<IActionResult> EditAsync(ProjectModel project)
        {
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            ProjectModel temp = await _projectsCosmosDbService.GetItemAsync(project.Id);
            if (temp.History == null)
                temp.History = new List<HistoryEntry>();
            project.History = new List<HistoryEntry>();

            if (project.StudentIds != null)
            {

                foreach (HistoryEntry history in temp.History)
                {
                    if (history.LogoutTime > new DateTime(1980, 1, 1)) project.History.Add(history);
                    else if (history.LogoutTime < new DateTime(1980, 1, 1) && project.StudentIds.Any(x => x == history.StudentId)) project.History.Add(history);
                    else if (history.LogoutTime < new DateTime(1980, 1, 1) && !project.StudentIds.Any(x => x == history.StudentId))
                    {
                        history.LogoutTime = DateTime.Now.ToUniversalTime();
                        project.History.Add(history);
                    }
                }
                if (project.StudentIds == null)
                    project.StudentIds = new List<string>();
                foreach (string id in project.StudentIds)
                {
                    if (!temp.History.Any(x => x.StudentId == id && x.LogoutTime < new DateTime(1980, 1, 1)))
                    {
                        project.History.Add(new HistoryEntry()
                        {
                            StudentId = id,
                            LoginTime = DateTime.Now.ToUniversalTime()
                        });
                    }
                }
            }


            ViewBag.Current = "Projekter";
            if (ModelState.IsValid)
            {
                await _projectsCosmosDbService.UpdateItemAsync(project.Id, project);
                if (project.IsComplete)
                {
                    try
                    {
                        TaskModel task = await _tasksCosmosDbService.GetItemAsync(project.Id);
                        if (task != null)
                        {
                            task.CurrentStatus = CurrentStatus.Færdigt;
                            await _tasksCosmosDbService.UpdateItemAsync(task.Id, task);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
                else
                {
                    try
                    {
                        TaskModel task = await _tasksCosmosDbService.GetItemAsync(project.Id);
                        if (task != null)
                        {
                            task.CurrentStatus = CurrentStatus.Accepteret;
                            await _tasksCosmosDbService.UpdateItemAsync(task.Id, task);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
                return RedirectToAction("Index");
            }

            return View(project);
        }

        [ActionName("Delete")]
        [Authorize(Roles = "Instructor,Administrator")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            ViewBag.Current = "Projekter";
            if (id == null || id.Length < 1)
            {
                return BadRequest();
            }

            ProjectModel project = await _projectsCosmosDbService.GetItemAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            project.IsDeleted = true;
            await _projectsCosmosDbService.UpdateItemAsync(id, project);
            return Redirect("/Project/Index");
        }

        [ActionName("ConvertFromTask")]
        [Authorize(Roles = "Instructor,Administrator")]
        public async Task<IActionResult> ConvertFromTaskToProject(TaskModel task)
        {
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            ViewBag.Current = "Opgaver";
            ProjectModel p = new ProjectModel()
            {
                Id = task.Id,
                CustomerId = task.CustomerId,
                InstructorId = task.InstructorId,
                Title = task.Title,
                Description = task.Description,
                StudentIds = new List<string>()
            };
            return View("Create", p);
        }

        [ActionName("AddMaterial")]
        public async Task<IActionResult> AddMaterialAsync(string id)
        {
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            ViewBag.Current = "Projekter";
            ViewBag.Project = await _projectsCosmosDbService.GetItemAsync(id);
            if (id == null || id.Length < 1)
            {
                return BadRequest();
            }

            ProjectModel project = await _projectsCosmosDbService.GetItemAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            return View();
        }

        [HttpPost]
        [ActionName("AddMaterial")]
        public async Task<IActionResult> AddMaterialAsync(MaterialModel material, string id)
        {
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            if (ModelState.IsValid)
            {
                ProjectModel project = await _projectsCosmosDbService.GetItemAsync(id);
                material.StudentId = User.Claims.First(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                material.AddedDate = DateTime.Now.ToUniversalTime();
                if (project.Materials == null)
                    project.Materials = new List<MaterialModel>();
                project.Materials.Add(material);
                await _projectsCosmosDbService.UpdateItemAsync(id, project);
            }

            return Redirect($"/Project/Details/{id}");
        }

        [ActionName("DeleteMaterial")]
        [Authorize(Roles = "Instructor,Administrator")]
        public async Task<IActionResult> DeleteMaterialAsync(string id, string materialName)
        {
            ViewBag.Current = "Projekter";
            if (id == null || id.Length < 1)
            {
                return BadRequest();
            }
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            ProjectModel project = await _projectsCosmosDbService.GetItemAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            project.Materials.First(x => x.Name == materialName).IsDeleted = true;
            await _projectsCosmosDbService.UpdateItemAsync(project.Id, project);
            return Redirect($"/Project/Details/{id}");
        }

        [ActionName("AssignToProject")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> AssignToProject(string projectId, string studentId)
        {
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            var project = await _projectsCosmosDbService.GetItemAsync(projectId);
            HistoryEntry history = new HistoryEntry() { LoginTime = DateTime.Now.ToUniversalTime(), StudentId = studentId };
            project.History.Add(history);
            project.StudentIds.Add(studentId);
            await _projectsCosmosDbService.UpdateItemAsync(projectId, project);
            return Redirect($"/Project/Details/{projectId}");
        }

        [ActionName("UnassignToProject")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UnassignToProject(string projectId, string studentId)
        {
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            var project = await _projectsCosmosDbService.GetItemAsync(projectId);
            project.History.First(x => x.StudentId == studentId && x.LogoutTime < new DateTime(1980, 1, 1)).LogoutTime = DateTime.Now.ToUniversalTime();
            project.StudentIds.Remove(studentId);
            await _projectsCosmosDbService.UpdateItemAsync(projectId, project);
            return Redirect($"/Project/Details/{projectId}");
        }
    }
}
