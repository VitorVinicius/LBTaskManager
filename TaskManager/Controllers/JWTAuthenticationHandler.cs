//using Microsoft.AspNetCore.Authentication;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Text.Encodings.Web;
//using System.Threading.Tasks;

//namespace TaskManager.Controllers
//{
//    public class JWTAuthenticationHandler : AuthenticationHandler<JWTAuthenticationOptions>
//    {
//        internal  const string SchemeName = "JWTAuthenticationScheme";

//        public IServiceProvider ServiceProvider { get; set; }

//        public JWTAuthenticationHandler(IOptionsMonitor<JWTAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IServiceProvider serviceProvider)
//            : base(options, logger, encoder, clock)
//        {
//            ServiceProvider = serviceProvider;
//        }

//        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
//        {
//            var headers = Request.Headers;
//            var token = "";

//            if (string.IsNullOrEmpty(token))
//            {
//                return Task.FromResult(AuthenticateResult.Fail("Token is empty"));
//            }

//            bool isValidToken = false; // check token here

//            if (!isValidToken)
//            {
//                return Task.FromResult(AuthenticateResult.Fail($"Balancer not authorize token : for token={token}"));
//            }

//            var claims = new[] { new Claim("token", token) };
//            var identity = new ClaimsIdentity(claims, nameof(JWTAuthenticationOptions));
//            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), this.Scheme.Name);
//            return Task.FromResult(AuthenticateResult.Success(ticket));
//        }

//    }

//    public class JWTAuthenticationOptions:Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions
//    {
//    }
//}
