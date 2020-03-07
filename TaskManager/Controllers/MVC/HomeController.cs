using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManager.Models;

namespace TaskManager.Controllers.MVC
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class HomeController : Controller
    {
        private User _currentUserData = null;
        private readonly TaskManagerContext _context;

        public HomeController(ITaskManagerContext context)
        {

            _context = (TaskManagerContext)context;
        }

        public IActionResult Index([FromServices]SigningConfigurations signingConfigurations,
            [FromServices]TokenConfigurations tokenConfigurations)
        {
            User UserData = GetCurrentUserData();
            ViewData.Add("UserData", UserData);
            Response.Cookies.Append(
                    "AccessToken",
                     API.UsersController.PerformLogon(signingConfigurations, tokenConfigurations, UserData).AccessToken,
                    new Microsoft.AspNetCore.Http.CookieOptions()
                    {
                        Path = "/"
                    }
                );
            return View();
        }
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        private User GetCurrentUserData()
        {

            try
            {
                _currentUserData = _currentUserData ?? _context.User.Find(long.Parse(User.Identity.Name));
            }
            catch
            {
                //Do nothing here yet
            }

            return _currentUserData;
        }

    }
}
