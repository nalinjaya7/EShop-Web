using EShopModels;
using EShopModels.Common;
using EShopWeb.Common;
using EShopWeb.Filters;
using EShopWeb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Web;

namespace EShopWeb.Controllers
{
    [EShopAuthorize(nameof(Roles.User))] 
    public class EShopUsersController : Controller
    { 
        private readonly HttpClient _httpClient;
        private readonly IOptions<EShopSystemConfig> _options; 
        private readonly IHttpContextAccessor contextAccessor;
        private readonly CryptoParamsProtector cryptoParams; 
        private readonly ILogger<EShopUsersController> _logger;
        private readonly JsonSerializerOptions options;

        public EShopUsersController(CryptoParamsProtector paramsProtector, IHttpClientFactory httpClientFactory, IOptions<EShopSystemConfig> config, IHttpContextAccessor httpContext, ILogger<EShopUsersController> logger)
        {
            cryptoParams = paramsProtector;
            contextAccessor = httpContext;
            _httpClient = httpClientFactory.CreateClient("EShopClient");
            _options = config;
            _logger = logger;
            string JWToken = contextAccessor.HttpContext.Session.GetString("JWToken");
            if (JWToken != null)
            { 
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")); 
                _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + JWToken); 
            }
            options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IncludeFields = false
            };
        }
  
        public async Task<ActionResult> Index(int? PageNumber)
        {
            List<EShopUserViewModel> users = new(); 
            int pagen = (PageNumber == null) ? 1 : (int)PageNumber;
            var response = await _httpClient.GetAsync("api/EShopUsers/GetEShopUsers/" + pagen);
            if (response.IsSuccessStatusCode)
            {
                var listPaginated = await JsonSerializer.DeserializeAsync<List<EShopUser>>(await response.Content.ReadAsStreamAsync(),options);
                foreach (EShopUser item in listPaginated)
                {
                    users.Add(new EShopUserViewModel(item.ID,item.UserName,item.FirstName,item.LastName, item.Address,item.Email,item.IsActive,item.ActivationCode, item.RoleName,item.RowVersion, cryptoParams)
                    { 
                        IsDeleted = item.IsDeleted 
                    });
                }
            }
            else
            {
                await ErrorMessages(response);
            }
            return View(users);
        }            
     
        [AllowAnonymous]
        [HttpGet]
        public IActionResult LoginUser(string ReturnUrl="/")
        {
            ViewData["Title"] = "Log in";
            LoginView model = new("","",false)
            {
                ReturnUrl = ReturnUrl
            };
            ViewData["ReturnUrl"] = HttpUtility.HtmlDecode(HttpContext.Request.QueryString.Value.Replace("?ReturnUrl=", ""));         
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost, ActionName("LoginUser")]
        public async Task<IActionResult> SignUser()
        {
            FormValueProvider formValProvider = new(BindingSource.Form, HttpContext.Request.Form, CultureInfo.InvariantCulture);
            LoginView user = new("", "", false);
            await TryUpdateModelAsync(user, "", formValProvider, p => p.Email, p => p.Password, p => p.RememberMe);

            ViewData["Title"] = "Log in";
            string tokenobj;
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PostAsJsonAsync("api/EShopUsers/ValidateToken", user);
                if (response.IsSuccessStatusCode)
                {
                    tokenobj = (string)await response.Content.ReadAsStringAsync();
                }
                else
                {
                    await ErrorMessages(response);
                    return View(user);
                }
                
                if (tokenobj != null)
                {
                    HttpContext.Session.SetString("JWToken", tokenobj);
                    JwtSecurityToken token = new JwtSecurityTokenHandler().ReadJwtToken(tokenobj);
                    var roleName = token.Claims.FirstOrDefault(f => f.Type == "RoleName").Value;
                    if(roleName != null && roleName == "Admin")
                    {
                        return RedirectToAction("Index", "Admin",new { Area="Admin" });
                    }
                    else if (roleName != null && roleName == "User")
                    {
                        return Redirect(user.ReturnUrl ?? "/");
                    }                   
                }
               
                ModelState.AddModelError("", "Invalid UserName or Password");
            }
            return View(user);
        }

        private static IEnumerable<Claim> GetUserClaims(EShopUser user)
        {
            IEnumerable<Claim> claims = new Claim[]
            {
                    new(ClaimTypes.Name, user.FullName),
                    new("USERNAME", user.UserName),
                     new Claim("EShopUserID",user.ID.ToString()),
                    new("EMAILID", user.Email),
                    new(ClaimTypes.Role,string.Join(",",user.RoleName))
            };
            return claims;
        }

        public IActionResult Logoff()
        {
            HttpContext.Session.Clear();
            return Redirect("~/Home/Index");
        }
  
        [CryptoValueProvider] 
        public async Task<ActionResult> Delete(int? id)
        {
            EShopUserViewModel EShopUser = null;
            if (id == null)
            {
                return new HttpStatusCodeWithBodyResult((int)HttpStatusCode.BadRequest, "User Error");
            }
            var response = await _httpClient.GetAsync("api/EShopUsers/GetEShopUser/" + id);
            if (response.IsSuccessStatusCode)
            {
               var EShopUserObj = await JsonSerializer.DeserializeAsync<EShopUser>(await response.Content.ReadAsStreamAsync(), options);
                EShopUser = new EShopUserViewModel(EShopUserObj.ID, EShopUserObj.UserName, EShopUserObj.FirstName, EShopUserObj.LastName, EShopUserObj.Address, EShopUserObj.Email, EShopUserObj.IsActive, EShopUserObj.ActivationCode, EShopUserObj.RoleName,EShopUserObj.RowVersion, cryptoParams)
                {
                    IsDeleted = EShopUserObj.IsDeleted, 
                    ModifiedDate = EShopUserObj.ModifiedDate,
                    CreatedDate = EShopUserObj.CreatedDate
                };
            }
            else
            {
                await ErrorMessages(response);
            }
            if (EShopUser == null)
            {
                return new NotFoundViewResult("UsersNotFound");
            }
            return View(EShopUser);
        }

        [CryptoValueProvider]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken] 
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            EShopUserViewModel EShopUser = null;
            var response = await _httpClient.DeleteAsync("api/EShopUsers/DeleteEShopUser/" + id);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                HttpResponseMessage response2 = await _httpClient.GetAsync("api/EShopUsers/GetEShopUser/" + id);
                if (response2.IsSuccessStatusCode)
                {
                    var EShopUserObj = await JsonSerializer.DeserializeAsync<EShopUser>(await response2.Content.ReadAsStreamAsync(), options);
                    EShopUser = new EShopUserViewModel(EShopUserObj.ID,EShopUserObj.UserName, EShopUserObj.FirstName, EShopUserObj.LastName, EShopUserObj.Address, EShopUserObj.Email, EShopUserObj.IsActive, EShopUserObj.ActivationCode, EShopUserObj.RoleName, EShopUserObj.RowVersion, cryptoParams)
                    {
                        IsDeleted = EShopUserObj.IsDeleted, 
                        ModifiedDate = EShopUserObj.ModifiedDate,
                        CreatedDate = EShopUserObj.CreatedDate
                    };
                }
                await ErrorMessages(response);
            }
            return View(EShopUser);            
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
            base.Dispose(disposing);
        }

        public async Task ErrorMessages(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                ModelState.AddModelError("", "Not Found");
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                ModelState.AddModelError("", "Bad Request");
            }
            else if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                ModelState.AddModelError("", "Internal Server Error");
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                ModelState.AddModelError("", "Un-Authorized");
            }
            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                ModelState.AddModelError("", "Service Unavailable");
            }
            string USERNAME = "", EMAILID = "", Name = "", exception = "";
            object exceptionobj = await JsonSerializer.DeserializeAsync<object>(await response.Content.ReadAsStreamAsync(), options);
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                Claim claim = HttpContext.User.Claims.Where(b => b.Type == "USERNAME").FirstOrDefault();
                USERNAME = claim.Value;

                claim = HttpContext.User.Claims.Where(b => b.Type == "EMAILID").FirstOrDefault();
                EMAILID = claim.Value;

                Name = HttpContext.User.Identity.Name;
            }

            _logger.Log(LogLevel.Error, new Exception(exceptionobj.ToString()), "{Name},{USERNAME},{EMAILID},{TypeName}", new object[] { Name, USERNAME, EMAILID, this.GetType().Name });
        }
    } 
}
