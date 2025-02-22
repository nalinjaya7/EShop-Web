using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Principal;

namespace EShopWeb.Filters
{
    public static class PermissionExtension
    {
        public static bool HasPermission(this Controller c,string claimvalue)
        {
            var userobj = c.HttpContext.User as ClaimsPrincipal;
            bool hasPermission = userobj.HasClaim(claimvalue, claimvalue);
            return hasPermission;
        }

        public static bool HasPermission(this IIdentity identity, string claimValue)
        {
            var userClaims = identity as ClaimsIdentity;
            bool HasPermissrion = userClaims.HasClaim(claimValue, claimValue);
            return HasPermissrion;
        } 
    }
}
