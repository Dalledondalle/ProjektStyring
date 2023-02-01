using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjektStyring.Models;
using ProjektStyring.Services;

namespace ProjektStyring.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClearStudentsFromProjects : ControllerBase
    {
        private IProjectsCosmosDbService _projectsCosmosDbService;

        public ClearStudentsFromProjects(IProjectsCosmosDbService projectsCosmosDbService)
        {
            _projectsCosmosDbService = projectsCosmosDbService;
        }
        [HttpGet]
        public async Task<IActionResult> ClearStudents()
        {
            var projects = await _projectsCosmosDbService.GetItemsAsync("SELECT * FROM c");
            if (projects != null)
            {
                DateTime today = DateTime.Now;
                foreach (ProjectModel project in projects)
                {
                    if (project.StudentIds != null)
                    {
                        while (project.StudentIds.Count > 0)
                        {
                            string currentStudent = project.StudentIds[0];
                            if(today.DayOfWeek == DayOfWeek.Friday)
                                project.History.First(x => x.StudentId == currentStudent && x.LogoutTime < new DateTime(1980, 1, 1)).LogoutTime = DateTime.UtcNow.AddHours(-4);
                            else
                                project.History.First(x => x.StudentId == currentStudent && x.LogoutTime < new DateTime(1980, 1, 1)).LogoutTime = DateTime.UtcNow.AddHours(-1);
                            project.StudentIds.Remove(currentStudent);
                        }
                        await _projectsCosmosDbService.UpdateItemAsync(project.Id, project);
                    }
                }
            }
            return Ok();
        }
    }
}
