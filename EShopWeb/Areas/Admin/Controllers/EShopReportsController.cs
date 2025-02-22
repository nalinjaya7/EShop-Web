using EShopModels;
using EShopModels.Common;
using EShopWeb.Common;
using EShopWeb.Filters;
using EShopWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace EShopWeb.Areas.Admin.Controllers
{
    [EShopAuthorize(nameof(Roles.Admin))]
    [Area("Admin")]
    public class EShopReportsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<EShopSystemConfig> _options;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly CryptoParamsProtector cryptoParams;
        private readonly ILogger<EShopReportsController> _logger;
        private readonly JsonSerializerOptions options;

        public EShopReportsController(CryptoParamsProtector paramsProtector, IHttpClientFactory httpClientFactory, IOptions<EShopSystemConfig> config, IHttpContextAccessor httpContext, ILogger<EShopReportsController> logger)
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
         
        public async Task ErrorMessages(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                ModelState.AddModelError("", "Not Found");
            }
            else if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                ModelState.AddModelError("", "Server Error");
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                ModelState.AddModelError("", "Un authorized");
            }
            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                ModelState.AddModelError("", "Service Unavailable");
            }
            string USERNAME = ""; string EMAILID = ""; string Name = "";
            string exception = await response.Content.ReadAsStringAsync();
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                Claim claim = HttpContext.User.Claims.Where(b => b.Type == "USERNAME").FirstOrDefault();
                USERNAME = claim.Value;

                claim = HttpContext.User.Claims.Where(b => b.Type == "EMAILID").FirstOrDefault();
                EMAILID = claim.Value;

                Name = HttpContext.User.Identity.Name;
            }
            _logger.Log(LogLevel.Error, new Exception(exception), "{Name},{USERNAME},{EMAILID},{TypeName}", new object[] { Name, USERNAME, EMAILID, GetType().Name });
        }
    }
}