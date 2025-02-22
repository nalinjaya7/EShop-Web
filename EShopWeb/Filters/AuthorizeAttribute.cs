using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Security.Claims;

namespace EShopWeb.Filters
{
    public class EShopAuthorizeAttribute : TypeFilterAttribute
    { 
        public EShopAuthorizeAttribute(params string[] claims) : base(typeof(EshopAuthorizeFilter))
        {
            Arguments = new object[] { claims };
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class EshopAuthorizeFilter : Attribute, IAuthorizationFilter
    {
        readonly string[] _claim;
        public EshopAuthorizeFilter(params string[] claim)
        {
            _claim = claim;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        { 
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
            {
                return;
            }

            bool isAuthenticated = context.HttpContext.User.Identity.IsAuthenticated; 
            if (isAuthenticated)
            {
                bool flagClaim = false;
                foreach (var item in _claim)
                {
                    if (context.HttpContext.User.HasClaim(ClaimTypes.Role, item))
                    {
                        flagClaim = true;
                    }
                }
                if (!flagClaim)
                {
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        controller = "EShopUsers",
                        action = "LoginUser",
                        area = "",
                        returnUrl = context.HttpContext.Request.GetDisplayUrl()
                    }));
                }
            }
            else
            {
                if (context.HttpContext.Request != null && context.HttpContext.Request.Headers != null && context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
                else
                { 
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        controller = "EShopUsers",
                        action = "LoginUser",
                        area = "",
                        returnUrl = context.HttpContext.Request.GetDisplayUrl()
                    }));
                }
            }
            return;
        }
    }
}
