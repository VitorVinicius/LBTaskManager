using System;
using System.Collections.Generic;
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
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class UsersController : Controller
    {
        private readonly TaskManagerContext _context;

        public UsersController(Models.ITaskManagerContext context)
        {
            _context = (TaskManagerContext)context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return await System.Threading.Tasks.Task.Run(() => { return RedirectToAction("Account"); });
        }


        // GET: Users/Signin/
        [AllowAnonymous]
        public async Task<IActionResult> Signin(string ReturnUrl)
        {
            ViewData.Add("ReturnUrl", ReturnUrl);
            return await System.Threading.Tasks.Task.Run(()=> { return View(new Signin()); });

        }

       


        // POST: Users/Signin/
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Signin(string ReturnUrl,[Bind("Email,Password")] Signin signin,
            [FromServices]SigningConfigurations signingConfigurations,
            [FromServices]TokenConfigurations tokenConfigurations)
        {
            ViewData["ReturnUrl"] = ReturnUrl;
            LogonResult logonResult = UsersAPIController.PerformLogon(signin, signingConfigurations, tokenConfigurations, _context);

            if (logonResult.Authenticated)
            {
                

                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(logonResult.Principal),
                    authProperties);

                this.Response.Cookies.Append(
                    "AccessToken",
                    logonResult.AccessToken,
                    new Microsoft.AspNetCore.Http.CookieOptions()
                    {
                        Path = "/"
                    }
                );

                if (ViewData["ReturnUrl"] != null)
                {
                    return Redirect(ViewData["ReturnUrl"].ToString());
                }
                else {

                    return RedirectToAction("Index", "Home");
                }
                

            }
            else
            {
                ModelState.AddModelError("Password", "Invalid Email/Password");
            }

            return View(signin);
        }




        // GET: Users/Signout/
        public async Task<IActionResult> Signout()
        {
            await HttpContext.SignOutAsync();
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
            return await System.Threading.Tasks.Task.Run(() => { return RedirectToAction("Signin", "Users"); });
        }


        // GET: Users/Details/5
        public async Task<IActionResult> Details()
        {
            var id = long.Parse(User.Identity.Name);
            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // GET: Users/Signup
        [AllowAnonymous]
        public IActionResult Signup()
        {
            return View();
        }

        // POST: Users/Signup
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Signup([Bind("Firstname,Lastname,Email,Password")] User user)
        {


            if (string.IsNullOrEmpty(user.Password))
            {
                ModelState.AddModelError("Password", "Password is required");
            }
            
            

            ValidateUserData(user);
            if (_context.User.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "This email is already associated with another account.");
            }

            if (ModelState.IsValid)
            {
                string pSalt = UsersAPIController.GeneratePasswordSalt();

                user.PasswordSalt = pSalt;
                string pwdHash = UsersAPIController.GeneratePasswordHash(user.Password, pSalt);

                user.PassworhHash = pwdHash;

                user.Password = null;

                user.RegistrationDate = DateTime.UtcNow;

                //user.LastUpdateDate = null; Keep empty at registration time.


                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        private void ValidateUserData(User user)
        {
            if (string.IsNullOrEmpty(user.Firstname?.Trim()))
            {
                ModelState.AddModelError("Firstname", "Firstname is required");
            }
            if (string.IsNullOrEmpty(user.Lastname?.Trim()))
            {
                ModelState.AddModelError("Lastname", "Lastname is required");
            }
            if (string.IsNullOrEmpty(user.Email?.Trim()))
            {
                ModelState.AddModelError("Email", "Email is required");
            }
            if (user.Email?.Contains("@")==false)
            {
                ModelState.AddModelError("Email", "Invalid email, must have an '@'.");
            }


            

            ModelState.Remove("PasswordSalt");
            ModelState.Remove("PassworhHash");
            ModelState.Remove("RegistrationDate");
            ModelState.Remove("RegistrationDate");
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Account()
        {

            var id = long.Parse(User.Identity.Name); 
            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return RedirectToAction(nameof(SignOut));
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Account([Bind("Firstname,Lastname,Email")] User user)
        {
            var id = long.Parse(User.Identity.Name);
            var _user = await _context.User.FindAsync(id);
            if (_user == null)
            {
                return NotFound();
            }
            user.Id = _user.Id;

            ValidateUserData(user);
            if (_context.User.Any(u => u.Email == user.Email && u.Id!= id))
            {
                ModelState.AddModelError("Email", "This email is already associated with another account.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete()
        {
            var id = long.Parse(User.Identity.Name);
            var user = await _context.User.FindAsync(id);
         

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed()
        {
            var id = long.Parse(User.Identity.Name);
            var user = await _context.User.FindAsync(id);


            if (user == null)
            {
                return NotFound();
            }
            _context.User.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(SignOut));
        }

        private bool UserExists(long id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}
