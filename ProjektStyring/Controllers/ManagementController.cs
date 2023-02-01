using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProjektStyring.Controllers
{
    [Authorize(Roles = "Instructor,Administrator")]
    public class ManagementController : Controller
    {
        public async Task<IActionResult> Index()
        {
            APITemp.RefreshCustomerList(await HttpContext.GetTokenAsync("access_token"));
            APITemp.RefreshUserList(await HttpContext.GetTokenAsync("access_token"));
            ViewBag.Current = "Administration";
            return View();
        }

        [HttpPost]
        [ActionName("GiveStockManagerRole")]
        public async Task<IActionResult> GiveStockManagerRole(string userId)
        {
            APITemp.AddStockManagerRole(await HttpContext.GetTokenAsync("access_token"), userId);
            Debug.WriteLine("Add");
            return Redirect("Index");
        }

        [HttpPost]
        [ActionName("RemoveStockManagerRole")]
        public async Task<IActionResult> RemoveStockManagerRole(string userId)
        {
            APITemp.RemoveStockManagerRole(await HttpContext.GetTokenAsync("access_token"), userId);
            Debug.WriteLine("Remove");
            return Redirect("Index");
        }
    }
}
