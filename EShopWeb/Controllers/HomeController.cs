using EShopModels;
using EShopWeb.Common;
using EShopWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace EShopWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<EShopSystemConfig> _options;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly CryptoParamsProtector cryptoParams; 
        private readonly JsonSerializerOptions options;

        public HomeController(CryptoParamsProtector paramsProtector, IHttpClientFactory httpClientFactory, IOptions<EShopSystemConfig> config, IHttpContextAccessor httpContext)
        {
            cryptoParams = paramsProtector;
            contextAccessor = httpContext;
            _httpClient = httpClientFactory.CreateClient("EShopClient");
            _options = config;
           
            string JWToken = contextAccessor.HttpContext.Session.GetString("JWToken");
            if (JWToken != null)
            {
                JwtSecurityToken Decryptedtoken = new JwtSecurityTokenHandler().ReadJwtToken(JWToken);
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + JWToken);
            }
            options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
                //,
                // IncludeFields = false
            };
        }

        public async Task<ActionResult> Index()
        {            
            return View();
        }
        
        public IActionResult Privacy()
        {            
            return PartialView();
        }

        public IActionResult AccessDenied()
        {
            return View();
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
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class PartialVM
    {
        public int Id { get; set; }
        [Required]
        public string? Prop1 { get; set; }
        [Required]
        public string? Prop2 { get; set; }
        [Required]
        public string? Prop3 { get; set; }
    }
}