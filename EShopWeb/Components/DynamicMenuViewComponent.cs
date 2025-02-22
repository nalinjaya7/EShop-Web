using EShopModels;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace EShopWeb.Components
{
    public class DynamicMenuViewComponent : ViewComponent
    {
        private readonly JsonSerializerOptions options;
        private readonly ILogger<DynamicMenuViewComponent> _logger;
        private readonly HttpClient _httpClient;
        public DynamicMenuViewComponent(IHttpClientFactory httpClientFactory, ILogger<DynamicMenuViewComponent> logger)
        {
            _httpClient = httpClientFactory.CreateClient("EShopClient");
            _logger = logger;
            options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            options.IncludeFields = false;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<ProductCategory> models = new(); 
            HttpResponseMessage response = await _httpClient.GetAsync("api/ProductCategories/GetAllProductCategories");

            if (response.IsSuccessStatusCode)
            {
                models = await JsonSerializer.DeserializeAsync<List<ProductCategory>>(await response.Content.ReadAsStreamAsync(), options);
            }
            else
            {
                await ErrorMessages(response);
            }
            return View(models);
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
            _logger.Log(LogLevel.Error, new Exception(exception), "{Name},{USERNAME},{EMAILID},{TypeName}", new object[] { Name, USERNAME, EMAILID, this.GetType().Name });
        }
    }
}
