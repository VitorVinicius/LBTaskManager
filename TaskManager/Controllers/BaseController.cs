using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;

namespace TaskManager
{
    public class BaseController:Controller
    {

        internal void SetPrincipal(IPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;
            if (HttpContext.User != null)
            {
                HttpContext.User = (ClaimsPrincipal)principal;
            }
        }



    }
}