using EShopModels;
using EShopWeb.Common;
using EShopWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace EShopWeb.Components
{
    public class SearchBoxViewComponent : ViewComponent
    {
        private readonly CryptoParamsProtector cryptoParams;
        private readonly JsonSerializerOptions options;
        private readonly ILogger<SearchBoxViewComponent> _logger;
        private readonly HttpClient _httpClient;
        public SearchBoxViewComponent(CryptoParamsProtector paramsProtector, IHttpClientFactory httpClientFactory, ILogger<SearchBoxViewComponent> logger)
        {
            _httpClient = httpClientFactory.CreateClient("EShopClient");
            _logger = logger;
            cryptoParams = paramsProtector;
            options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IncludeFields = false
            };
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<SearchBoxModel> models = new();
            HttpResponseMessage response = await _httpClient.GetAsync("api/ProductCategories/GetSearchBoxItems");

            if (response.IsSuccessStatusCode)
            {
                var modelsobj = await JsonSerializer.DeserializeAsync<List<SearchBox>>(await response.Content.ReadAsStreamAsync(), options);
                foreach (SearchBox sitem in modelsobj)
                {
                    models.Add(new SearchBoxModel(sitem.ID, sitem.Name, sitem.Type, cryptoParams) {Prices= sitem.Prices });
                }
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
