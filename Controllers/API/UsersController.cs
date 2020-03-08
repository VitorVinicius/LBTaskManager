using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Models;

namespace TaskManager.Controllers.API
{


    /// <summary>
    /// User Management routes
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : ControllerBase
    {
        private readonly TaskManagerContext _context;
        private User _currentUserData = null;

        public UsersController(ITaskManagerContext context)
        {
            _context = (TaskManagerContext)context;
        }

        public static User CheckSignin(Signin signin, TaskManagerContext context)
        {
            User user = null;
            if (signin.Email != null && signin.Password != null)
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
            return Encoding.UTF8.GetString(bytes); ;
        }



        /// <summary>
        /// Perform logon and returns JWT Token Info.
        /// </summary>
        /// <param name="Signin">Logon data. Email and password</param>
        /// <param name="signingConfigurations">Service provided signing configurations</param>
        /// <param name="tokenConfigurations">Service provided token configurations</param>
        /// <returns>JWT Token Info</returns>
        [AllowAnonymous]
        [HttpPost]
        [ActionName("Logon")]
        [ProducesResponseType(200, Type = typeof(LogonResult))]
        [ProducesResponseType(401, Type = typeof(LogonResult))]
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
        /// <summary>
        /// Process User provided Signin data
        /// </summary>
        /// <param name="Signin">Logon data. Email and password</param>
        /// <param name="signingConfigurations">Service provided signing configurations</param>
        /// <param name="tokenConfigurations">Service provided token configurations</param>
        /// <param name="context">The Task Manager Database Context</param>
        /// <returns>Logon results with JWT Data</returns>
        [NonAction]
        public static LogonResult PerformLogon(Signin Signin, SigningConfigurations signingConfigurations, TokenConfigurations tokenConfigurations, TaskManagerContext context)
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
                return PerformLogon(signingConfigurations, tokenConfigurations, user);
            }
        }
        /// <summary>
        /// Process system provided User Signin data
        /// </summary>
        /// <param name="signingConfigurations">Service provided signing configurations</param>
        /// <param name="tokenConfigurations">Service provided token configurations</param>
        /// <param name="user">User to get logon data and JWT Token</param>
        /// <returns>Logon results with JWT Data</returns>
        [NonAction]
        public static LogonResult PerformLogon(SigningConfigurations signingConfigurations, TokenConfigurations tokenConfigurations, User user)
        {
            if (user == null) return null;
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


            )
            {
                Principal = new ClaimsPrincipal(identity)
            };
        }




        // GET: api/Users/Get
        ///<summary>Get current logged user data</summary>
        ///<returns>User Data</returns>
        [HttpGet]
        public async Task<ActionResult<User>> Get()
        {
            User UserData = GetCurrentUserData();
            var _knownUser = _context.User.Where(x => x.Id == UserData.Id)?.FirstOrDefault();
            return await System.Threading.Tasks.Task.Run(() => { return Ok(_knownUser); });
        }

        /// <summary>
        /// Get Current Logged User Data
        /// </summary>
        /// <returns></returns>
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



        // PUT: api/Users/Update
        ///<summary>Update current logged User</summary>
        ///<returns>New User Data Stored Changes</returns>
        [HttpPut]
        [ProducesResponseType(204, Type = typeof(void))]
        [ProducesResponseType(404, Type = typeof(void))]
        public async Task<IActionResult> Update(User User)
        {
            
            User UserData = GetCurrentUserData();

            UserData.Firstname = User.Firstname;
            UserData.Lastname = User.Lastname;
            UserData.Email = User.Email;
            UserData.LastUpdateDate = DateTime.UtcNow;


            if (string.IsNullOrEmpty(User.Password) == false)
            {
                //Process password change
                string pSalt = GeneratePasswordSalt();

                User.PasswordSalt = pSalt;
                string pwdHash = GeneratePasswordHash(User.Password, pSalt);

                User.PassworhHash = pwdHash;

                User.Password = null;
            }


            List<string> errors = ValidateUserData(User);

            if (_context.User.Any(u => u.Email == User.Email && u.Id!= UserData.Id))
            {
                errors?.Add("This email is already associated with another account.");
            }

            if (errors?.Count > 0)
            {
                throw new AggregateException(errors.Select(x => new Exception(x)));
            }
            else
            {


                _context.Entry(UserData).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }

                return NoContent();
            }
        }

        // POST: api/Users/Create
        ///<summary>Create user</summary>
        ///<returns>New User Created</returns>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(void))]
        [ProducesResponseType(400, Type = typeof(ProblemDetails))]
        [AllowAnonymous]
        public async Task<ActionResult<User>> Create(User User)
        {

            List<string> errors = ValidateUserData(User);

            if (_context.User.Any(u => u.Email == User.Email))
            {
                errors?.Add("This email is already associated with another account.");
            }

            if (string.IsNullOrEmpty(User.Password?.Trim()))
            {
                errors?.Add("Password must be provided");
            }

            if (errors?.Count > 0)
            {

               
                ProblemDetails problemDetail = new ProblemDetails()
                {
                    Detail = string.Join('\n', errors),
                    Instance = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Status = 400,
                    Title = "Invalid Data Provided",

                };
                int errIndex = 0;
                foreach (string error in errors)
                {
                    problemDetail.Extensions?.Add(new KeyValuePair<string, object>(errIndex.ToString(), error));
                }

               
                return base.BadRequest(problemDetail);
            }
            else
            {
                //Store Credentials in Hash Format with password plain text
                string pSalt = GeneratePasswordSalt();

                User.PasswordSalt = pSalt;
                string pwdHash = GeneratePasswordHash(User.Password, pSalt);

                User.PassworhHash = pwdHash;

                User.Password = null;

                _context.User.Add(User);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetUser", new { id = User.Id }, User);
            }
            
        }


        /// <summary>
        /// Validate user provided data
        /// </summary>
        /// <param name="user">User's data</param>
        private List<string>  ValidateUserData(User user)
        {
            List<string> errors = new List<string>();
            if (string.IsNullOrEmpty(user.Firstname?.Trim()))
            {
                errors.Add("Firstname is required");
            }
            if (string.IsNullOrEmpty(user.Lastname?.Trim()))
            {
                errors.Add("Lastname is required");
            }
            if (string.IsNullOrEmpty(user.Email?.Trim()))
            {
                errors.Add( "Email is required");
            }
            if (user.Email?.Contains("@") == false)
            {
                errors.Add("Invalid email, must have an '@'.");
            }

            


            return errors;
        }


        // DELETE: api/Users/Delete
        ///<summary>Delete current logged user</summary>
        ///<returns>User Data before deletion</returns>
        [HttpDelete]
        [ProducesResponseType(200, Type = typeof(User))]
        [ProducesResponseType(404, Type = typeof(void))]
        public async Task<ActionResult<User>> Delete()
        {
            User UserData = GetCurrentUserData();
           

            _context.User.Remove(UserData);
            await _context.SaveChangesAsync();

            return UserData;
        }



        /// <summary>
        /// Check user exists
        /// </summary>
        /// <param name="id">The User's Id</param>
        /// <returns>true if exists, false if not</returns>
        private bool UserExists(long id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}

