using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EShopWeb.Controllers
{
    public class AccountController : Controller
    { 
        private readonly HttpClient _httpClient; 
        private readonly IHttpContextAccessor contextAccessor;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IHttpClientFactory httpClientFactory,IHttpContextAccessor httpContext, ILogger<AccountController> logger)
        {
            _logger = logger;
              contextAccessor = httpContext;
            _httpClient = httpClientFactory.CreateClient("EShopClient");  
            string JWToken = contextAccessor.HttpContext.Session.GetString("JWToken");
            if (JWToken != null)
            { 
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + JWToken);
            }
        }

        public ActionResult LogOut()
        {
            _logger.Log(LogLevel.Information, "LogOut");
            CookieOptions option = new();
            option.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Append("Cookie1", "", option);
             
            CookieOptions option2 = new();
            option2.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Append("logindetail", "", option2);

            CookieOptions option3 = new();
            option3.Expires = DateTime.Now.AddYears(-1); 
             string USERNAME = ""; string EMAILID = ""; string Name = ""; 
            if (HttpContext.User.Identity.IsAuthenticated)
            { 

               Claim claim = HttpContext.User.Claims.Where(b => b.Type == "USERNAME").FirstOrDefault();
                USERNAME = claim.Value;

                claim = HttpContext.User.Claims.Where(b => b.Type == "EMAILID").FirstOrDefault();
                EMAILID = claim.Value;

                Name = HttpContext.User.Identity.Name;
            }
            _logger.Log(LogLevel.Information, new Exception(""), "{Name},{USERNAME},{EMAILID},{TypeName}", new object[] { Name, USERNAME, EMAILID, this.GetType().Name });

            return RedirectToAction("Login", "Account", null);
        }
    }
    
    public interface IFormsAuthenticationService
    {
        void SignOut();
    }

    public class FormsAuthenticationService : IFormsAuthenticationService
    {
        public void SignOut()
        {
            //FormsAuthentication.SignOut();
        }
    }
}
