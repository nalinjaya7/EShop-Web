using EShopModels;
using EShopWeb.Common;
using EShopWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace EShopWeb.Components
{
    public class ProductsByCategoryViewComponent : ViewComponent
    {
        private readonly JsonSerializerOptions options;
        private readonly CryptoParamsProtector cryptoParams;
        private readonly ILogger<ProductsByCategoryViewComponent> _logger;
        private readonly HttpClient _httpClient;
        public ProductsByCategoryViewComponent(CryptoParamsProtector paramsProtector, IHttpClientFactory httpClientFactory, ILogger<ProductsByCategoryViewComponent> logger)
        {
            cryptoParams = paramsProtector;
            _httpClient = httpClientFactory.CreateClient("EShopClient");
            _logger = logger;
            options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            options.IncludeFields = false;
        }

        public async Task<IViewComponentResult> InvokeAsync(int CategoryID)
        {
            List<ProductViewModel> product = new();
            var response = await _httpClient.GetAsync("api/Products/GetProductsByCategory?SubCategoryID=" + CategoryID);
            if (response.IsSuccessStatusCode)
            {
                var productobj = await JsonSerializer.DeserializeAsync<List<Product>>(await response.Content.ReadAsStreamAsync(), options);
                foreach (Product item in productobj)
                { 
                    product.Add(new ProductViewModel(item.ID, CategoryID, item.ProductCategoryID, item.Name, item.BarCode, item.ItemCode, 0, 0, false, 0, item.ProductImage, item.RowVersion, cryptoParams)
                    {
                        IsDeleted = item.IsDeleted,
                        Inventories = item.Inventories.Select(i => new InventoryViewModel(i.ID, i.Code, i.ProductID, i.UnitChartID, i.Quantity, i.BatchID, i.SellingPrice, i.PurchasePrice, i.ReservedQuantity, i.RowVersion, cryptoParams)
                        {                            
                            UnitChart = new UnitChartViewModel(0, (i.UnitChart != null) ? i.UnitChart.UnitTypeID : 0, (i.UnitChart != null) ? i.UnitChart.ProductID : 0, (i.UnitChart != null) ? i.UnitChart.Quantity : 0, (i.UnitChart != null) ? i.UnitChart.UnitTypeName : "", (i.UnitChart != null) ? i.UnitChart.RowVersion : new byte[] { }, cryptoParams)
                            { UnitType = new UnitTypeViewModel(0, "", "",((i.UnitChart != null) ? i.UnitChart.UnitType.IsBaseUnit : false), Array.Empty<byte>(), cryptoParams) }
                        }).ToList()
                    });

                }
            }
            else
            {
                await ErrorMessages(response);
            }
            return View(product);
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
