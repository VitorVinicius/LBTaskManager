using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [Route("api/Users/{action=Index}/{id?}")]
    [ApiController]
    public class UsersAPIController : ControllerBase
    {
        private readonly TaskManagerContext _context;

        public UsersAPIController(ITaskManagerContext context)
        {
            _context = (TaskManagerContext)context;
        }
        public static User CheckSignin(Signin signin, TaskManagerContext context)
        {
            User user = null;
            if (signin.Email != null && signin.Password!=null)
            {
                user = context.User.Where(x => x.Email == signin.Email.Trim()).FirstOrDefault();

                if (user != null)
                {
                    var currentHash = GeneratePasswordHash(signin.Password, user.PasswordSalt);
                    var expectedHash = user.PassworhHash;

                    if (currentHash == null || currentHash != expectedHash)
                    {
                        user = null;
                    }
                }
                

            }

            return user;
        }



        public static string GeneratePasswordHash(string password, string pSalt)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(password + pSalt);
            SHA256Managed sHA256MngedStr = new SHA256Managed();
            byte[] hashBuff = sHA256MngedStr.ComputeHash(bytes);
            string pwdHash = Convert.ToBase64String(hashBuff);
            return pwdHash;
        }

        public static string GeneratePasswordSalt()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[512];
            rng.GetBytes(bytes);
            return System.Text.Encoding.UTF8.GetString(bytes); ;
        }


        

        [AllowAnonymous]
        [HttpPost]
        public Task<ActionResult<LogonResult>> Logon(
            [FromBody]Signin Signin,
            [FromServices]SigningConfigurations signingConfigurations,
            [FromServices]TokenConfigurations tokenConfigurations)
        {

            var result = PerformLogon(Signin, signingConfigurations, tokenConfigurations, _context);

            if (result.Authenticated)
            {
                return System.Threading.Tasks.Task.Run<ActionResult<LogonResult>>(() =>
                {
                    return Ok(result);
                });
            }
            else
            {
                return System.Threading.Tasks.Task.Run<ActionResult<LogonResult>>(() =>
                {
                    return StatusCode(401, result);
                });
            }
            



        }

        public static LogonResult PerformLogon(Signin Signin, SigningConfigurations signingConfigurations, TokenConfigurations tokenConfigurations,TaskManagerContext context)
        {
            bool isLoginValid = false;
            User user = null;
            if (Signin != null && !string.IsNullOrEmpty(Signin.Email) && !string.IsNullOrEmpty(Signin.Password))
            {
                user = CheckSignin(Signin, context);
                isLoginValid = user != null;
            }

            if (!isLoginValid)
            {
                return new LogonResult
                {
                    Authenticated = false,
                    Message = "Authorization Failed."
                };
            }
            else
            {
                ClaimsIdentity identity = new ClaimsIdentity(
                    new GenericIdentity(user.Id.ToString(), "Login"),
                    new[] {
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                        new Claim(JwtRegisteredClaimNames.UniqueName, user.Id.ToString())
                    }
                );

               

                DateTime jwtDateOfCreation = DateTime.Now;
                DateTime jwtDateOfExpiration = jwtDateOfCreation +
                    TimeSpan.FromSeconds(tokenConfigurations.Seconds);

                var jwtSecHandler = new JwtSecurityTokenHandler();
                var securityToken = jwtSecHandler.CreateToken(new SecurityTokenDescriptor
                {
                    Issuer = tokenConfigurations.Issuer,
                    Audience = tokenConfigurations.Audience,
                    SigningCredentials = signingConfigurations.SigningCredentials,
                    Subject = identity,
                    NotBefore = jwtDateOfCreation,
                    Expires = jwtDateOfExpiration
                });
               
                var token = jwtSecHandler.WriteToken(securityToken);

                return new LogonResult(
                        true,
                        jwtDateOfCreation.ToString("yyyy-MM-dd HH:mm:ss"),
                        jwtDateOfExpiration.ToString("yyyy-MM-dd HH:mm:ss"),
                        token,
                        "OK"


                ) {
                    Principal = new ClaimsPrincipal(identity)
                };
            }
        }

        private bool UserExists(long id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}

