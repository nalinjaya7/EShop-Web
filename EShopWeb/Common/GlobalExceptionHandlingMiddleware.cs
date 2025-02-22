using System.Security.Claims;

namespace EShopWeb.Common
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
        public GlobalExceptionHandlingMiddleware(RequestDelegate request, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _logger = logger;
            _next = request; 
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch(Exception ex)
            { 
                string USERNAME = ""; string EMAILID = ""; string Name = "";
                HttpContextAccessor contextAccessor = new HttpContextAccessor();
                if (contextAccessor.HttpContext.User.Identity.IsAuthenticated)
                { 
                   Claim claim = contextAccessor.HttpContext.User.Claims.Where(b => b.Type == "USERNAME").FirstOrDefault();
                    USERNAME = claim.Value;

                    claim = contextAccessor.HttpContext.User.Claims.Where(b => b.Type == "EMAILID").FirstOrDefault();
                    EMAILID = claim.Value;

                    Name = contextAccessor.HttpContext.User.Identity.Name;
                }
                _logger.Log(LogLevel.Error,ex, "{Name},{USERNAME},{EMAILID},{TypeName}", new object[] { Name, USERNAME, EMAILID, this.GetType().Name }); 
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
        {
            httpContext.Items["exception"] = ex;
            PathString originalPath = httpContext.Request.Path;
            QueryString originalQueryString = httpContext.Request.QueryString;
            try
            {
                httpContext.Request.Path = "/Error/Error";
                httpContext.Request.QueryString = httpContext.Request.QueryString.Add("StatusCode",httpContext.Response.StatusCode.ToString());
                await _next(httpContext);
            }            
            finally
            {
                httpContext.Request.Path = originalPath;
                httpContext.Request.QueryString = originalQueryString;
            }
        }
    } 
}
