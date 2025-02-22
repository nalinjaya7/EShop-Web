using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Security.Claims;

namespace EShopWeb.Filters
{
    public class EshopExceptionHandler : ExceptionFilterAttribute
    { 
        private string Name = string.Empty;
        private string USERNAME = string.Empty;
        private string EMAILID = string.Empty;
        private readonly ILogger<EshopExceptionHandler> _logger;

        public EshopExceptionHandler(ILogger<EshopExceptionHandler> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext filterContext)
        {
            if(filterContext.ExceptionHandled)
            {
                return;
            }
            Exception exec = filterContext.Exception;
            while (exec.InnerException != null)
            {
                exec = exec.InnerException;
            }
            var modelResult = new ViewResult { ViewName = "Error" };
            var modelMetaData = new EmptyModelMetadataProvider();
            filterContext.Result = new ViewResult()
            {
                ViewName = "~/Views/Shared/Error.cshtml",
                ViewData = new ViewDataDictionary(modelMetaData, filterContext.ModelState)
            };
            ((ViewResult)filterContext.Result).ViewData.Add("HandleException", new Exception("Internal Server Error"));
            filterContext.ExceptionHandled = true;
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            { 
               Claim claim = filterContext.HttpContext.User.Claims.Where(b => b.Type == "USERNAME").FirstOrDefault();
                USERNAME = claim.Value;

                claim = filterContext.HttpContext.User.Claims.Where(b => b.Type == "EMAILID").FirstOrDefault();
                EMAILID = claim.Value;

                Name = filterContext.HttpContext.User.Identity.Name;
            }

            _logger.Log(LogLevel.Error, filterContext.Exception, "{Name},{USERNAME},{EMAILID},{TypeName}", new object[] { Name, USERNAME, EMAILID, this.GetType().Name });
 
            base.OnException(filterContext);
        }

        public override async Task OnExceptionAsync(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
            {
                return;
            }
            Exception exec = filterContext.Exception;
            while (exec.InnerException != null)
            {
                exec = exec.InnerException;
            }
            var modelResult = new ViewResult { ViewName = "Error" };
            var modelMetaData = new EmptyModelMetadataProvider();
            filterContext.Result = new ViewResult()
            {
                ViewName = "~/Views/Shared/Error.cshtml",
                ViewData = new ViewDataDictionary(modelMetaData, filterContext.ModelState)
            };
            ((ViewResult)filterContext.Result).ViewData.Add("HandleException", new Exception("Internal Server Error"));
            filterContext.ExceptionHandled = true;
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            { 
               Claim claim = filterContext.HttpContext.User.Claims.Where(b => b.Type == "USERNAME").FirstOrDefault();
                USERNAME = claim.Value;

                claim = filterContext.HttpContext.User.Claims.Where(b => b.Type == "EMAILID").FirstOrDefault();
                EMAILID = claim.Value;

                Name = filterContext.HttpContext.User.Identity.Name;
            }

            _logger.Log(LogLevel.Error, filterContext.Exception, "{Name},{USERNAME},{EMAILID},{TypeName}", new object[] { Name, USERNAME, EMAILID, this.GetType().Name });

            await base.OnExceptionAsync(filterContext); 
        }
    }
}