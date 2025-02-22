using EShopModels;
using EShopModels.Common;
using EShopWeb.Common;
using EShopWeb.Filters;
using EShopWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace EShopWeb.Controllers
{
    [EShopAuthorize(nameof(Roles.User))] 
    public class ProductsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<EShopSystemConfig> _options; 
        private readonly IHttpContextAccessor contextAccessor;
        private readonly CryptoParamsProtector cryptoParams;  
        private readonly ILogger<ProductsController> _logger;
        private readonly JsonSerializerOptions options;
        public ProductsController(CryptoParamsProtector paramsProtector, IHttpClientFactory httpClientFactory, IOptions<EShopSystemConfig> config, IHttpContextAccessor httpContext, ILogger<ProductsController> logger)
        { 
            cryptoParams = paramsProtector;
            contextAccessor = httpContext;
            _httpClient = httpClientFactory.CreateClient("EShopClient");
            _options = config;
            _logger = logger;
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
            };
        }
                 
        public ActionResult ProductUpload()
        { 
            return View();
        }

        [HttpPost,HttpGet]
        public async Task<ActionResult> Index()
        {
            Product product = new(0, 0, "", "", "", 0, 0, false, 0);
            if (Request.HasFormContentType)
            {
                FormValueProvider formValue = new(BindingSource.Form, Request.Form, CultureInfo.InvariantCulture);   
                ViewData["Name"] = Request.Form["Name"].ToString();
                ViewData["ItemCode"] = Request.Form["ItemCode"].ToString();
                await TryUpdateModelAsync<Product>(product, "", formValue, p => p.ProductSubCategoryID, p => p.Name, p => p.ItemCode);
            }
            ModelState.Clear();

            List<ProductViewModel> models = new(); 
            int pageNo = 1;
            var response = await _httpClient.PostAsJsonAsync("api/Products/ProductSearch/"+ pageNo,product);

            if (response.IsSuccessStatusCode)
            {
               var products = await JsonSerializer.DeserializeAsync<List<Product>>(await response.Content.ReadAsStreamAsync(),options);
                foreach (Product item in products)
                { 
                    models.Add(new ProductViewModel(item.ID,item.ProductSubCategoryID, item.ProductCategoryID, item.Name, item.BarCode, item.ItemCode, 0, 0, false, 0, item.ProductImage, item.RowVersion, cryptoParams)
                    { 
                        IsDeleted = item.IsDeleted
                    });
                }
            }
            else
            {
                await ErrorMessages(response);
            }

            List<ProductSubCategoryViewModel> ProductSubCategories = new();
            var responses = await _httpClient.GetAsync("api/ProductSubCategories/GetSortedProductSubCategories");
            if (responses.IsSuccessStatusCode)
            {
                var ProductSubCategoriesObj = await JsonSerializer.DeserializeAsync<List<ProductSubCategory>>(await responses.Content.ReadAsStreamAsync(), options);
                foreach (ProductSubCategory itemObj in ProductSubCategoriesObj)
                {
                    ProductSubCategories.Add(new ProductSubCategoryViewModel(itemObj.ID,itemObj.ProductCategoryID,itemObj.Name,itemObj.RowVersion, cryptoParams)
                    { 
                    });
                }
            }
            else
            {
                await ErrorMessages(responses);
            }
            ProductSubCategories.Insert(0, new ProductSubCategoryViewModel(0,0,"All",Array.Empty<byte>(),cryptoParams) {});
            ViewData["ProductSubCategoryID"] = new SelectList(ProductSubCategories, "ID", "Name", product.ProductSubCategoryID);          
            return View("Index", models);
        }
   
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetProductsViewBySubCategory(int SubCategory)
        {
            ViewData["CategoryID"] = SubCategory;
 
            return View();
        }

        [CryptoValueProvider]
        [AllowAnonymous]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeWithBodyResult((int)HttpStatusCode.BadRequest, "Products Error");
            }

            ProductViewModel product =null;
            var response = await _httpClient.GetAsync("api/Products/GetProduct/" + id);
            if (response.IsSuccessStatusCode)
            {
               var productObj = await JsonSerializer.DeserializeAsync<Product>(await response.Content.ReadAsStreamAsync(),options);
                product = new ProductViewModel(productObj.ID, productObj.ProductSubCategoryID, productObj.ProductCategoryID, productObj.Name, productObj.BarCode, productObj.ItemCode, 0, 0, false, 0, productObj.ProductImage, productObj.RowVersion, cryptoParams)
                {
                    IsDeleted = productObj.IsDeleted,
                    Inventories = productObj.Inventories.Select(i => new InventoryViewModel(i.ID, i.Code, i.ProductID, i.UnitChartID, i.Quantity, i.BatchID, i.SellingPrice, i.PurchasePrice, i.ReservedQuantity, i.RowVersion, cryptoParams) { UnitChart = new UnitChartViewModel(0, i.UnitChart.UnitTypeID, i.UnitChart.ProductID, i.UnitChart.Quantity, i.UnitChart.UnitTypeName, i.UnitChart.RowVersion, cryptoParams) { UnitType = new UnitTypeViewModel(0, "", "", i.UnitChart.UnitType.IsBaseUnit, Array.Empty<byte>(), cryptoParams) } }).ToList()
                };
            }
            else
            {
                await ErrorMessages(response);
            }
            if (product == null)
            {
                return new NotFoundViewResult("ProductsNotFound");
            }
            return View(product);
        }
 
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetProductSubCategories(int CategoryId)
        {
            List<ProductSubCategoryViewModel> ProductSubCategories = new();
            var responsea = await _httpClient.GetAsync("api/ProductSubCategories/GetSubCategoriesByCategory/"+ CategoryId);
            if (responsea.IsSuccessStatusCode)
            {
                List<ProductSubCategory> listPaginated = await JsonSerializer.DeserializeAsync<List<ProductSubCategory>>(await responsea.Content.ReadAsStreamAsync(), options);
                foreach (ProductSubCategory item in listPaginated)
                {
                    ProductSubCategories.Add(new ProductSubCategoryViewModel(item.ID, CategoryId, item.Name,item.RowVersion,cryptoParams)
                    {
                        
                    });
                }
            }
            else
            {
                await ErrorMessages(responsea);
            }
            return Json(new { SubCategories = new SelectList(ProductSubCategories, "ID", "Name") });
        }
          
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetUnitTypes()
        {
            List<UnitType> unitTypeModels = new();
            var responsea = await _httpClient.GetAsync("api/UnitTypes/GetUnitTypes");
            if (responsea.IsSuccessStatusCode)
            {
                unitTypeModels = await JsonSerializer.DeserializeAsync<List<UnitType>>(await responsea.Content.ReadAsStreamAsync(), options);
            }
            else
            {
                await ErrorMessages(responsea);
            }
            return Json(new { UnitTypes = unitTypeModels });
        }

        [HttpGet]
        public async Task<JsonResult> GetInventoryByCode(string id)
        {
            Inventory inventory = new(id, 0, 0, 0.00m, 0, 0.00m, 0.00m, 0.00m);
            var response = await _httpClient.GetAsync("api/Inventories/GetInventoryByCode/" + id);
            if (response.IsSuccessStatusCode)
            {
                inventory = await JsonSerializer.DeserializeAsync<Inventory>(await response.Content.ReadAsStreamAsync(), options);
            }
            else
            {
                await ErrorMessages(response);
            }
            return Json(new { Inventory = inventory });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetUnitChartsByProduct(int ProductID)
        {
            List<UnitChart> unitChartModels = new();
            var responsea = await _httpClient.GetAsync("api/UnitCharts/GetUnitChartsByProduct/"+ ProductID);
            if (responsea.IsSuccessStatusCode)
            {
                unitChartModels = await JsonSerializer.DeserializeAsync<List<UnitChart>>(await responsea.Content.ReadAsStreamAsync(), options);
            }
            else
            {
                await ErrorMessages(responsea);
            }
            return Json(new { Unitcharts = unitChartModels });
        }

        [HttpPut]
        public async Task<JsonResult> UpdateUnitChart(int ProductID,[FromBody]UnitChart[] unitChartModel)
        {
            List<UnitChart> unitChartModels = new();
            var responsea = await _httpClient.PutAsJsonAsync("api/UnitCharts/UpdateUnitChart/" + ProductID, unitChartModel);
            if (responsea.IsSuccessStatusCode)
            {
                unitChartModels = await JsonSerializer.DeserializeAsync<List<UnitChart>>(await responsea.Content.ReadAsStreamAsync(), options);
            }
            else
            {
                await ErrorMessages(responsea);
            }
            return Json(new { Unitcharts = unitChartModels });
        }

        [HttpDelete]
        public async Task<JsonResult> DeleteUnitChart(int UnitchartID,int ProductID)
        {
            List<UnitChart> unitChartModels = new();
            var responsea = await _httpClient.DeleteAsync("api/UnitCharts/DeleteUnitChart/" + UnitchartID + "/" + ProductID);
            if (responsea.IsSuccessStatusCode)
            {
                unitChartModels = await JsonSerializer.DeserializeAsync<List<UnitChart>>(await responsea.Content.ReadAsStreamAsync(), options);
            }
            else
            {
                await ErrorMessages(responsea);
            }
            return Json(new { unitcharts = unitChartModels });
        }
        
        [CryptoValueProvider]
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetProductDetails(int ProductID)
        {
            List<Inventory> inventories = new();
            //object objsellingprices = new();

            var response = await _httpClient.GetAsync("api/Products/GetProductInventories/" + ProductID);
            if (response.IsSuccessStatusCode)
            {
                inventories = await JsonSerializer.DeserializeAsync<List<Inventory>>(await response.Content.ReadAsStreamAsync(), options);
            }
            else
            {
                await ErrorMessages(response);
            }
            return Json(new { inventories });
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
