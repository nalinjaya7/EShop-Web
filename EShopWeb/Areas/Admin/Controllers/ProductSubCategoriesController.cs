using EShopModels;
using EShopModels.Common;
using EShopWeb.Common;
using EShopWeb.Filters;
using EShopWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace EShopWeb.Areas.Admin.Controllers
{
    [EShopAuthorize(nameof(Roles.Admin))]
    [Area("Admin")]
    public class ProductSubCategoriesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<EShopSystemConfig> _options;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly CryptoParamsProtector cryptoParams;
        private readonly ILogger<ProductSubCategoriesController> _logger;
        private readonly JsonSerializerOptions options;

        public ProductSubCategoriesController(CryptoParamsProtector paramsProtector, IHttpClientFactory httpClientFactory, IOptions<EShopSystemConfig> config, IHttpContextAccessor httpContext, ILogger<ProductSubCategoriesController> logger)
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

        public async Task<ActionResult> Index(int? pagenumber)
        {
            List<ProductSubCategoryViewModel> productSubCategories = new(); 
            int pageNo = pagenumber == null ? 1 : (int)pagenumber;
            var response = await _httpClient.GetAsync("api/ProductSubCategories/GetProductSubCategories/" + pageNo);

            if (response.IsSuccessStatusCode)
            {
               var productsubCategory = await JsonSerializer.DeserializeAsync<List<ProductSubCategory>>(await response.Content.ReadAsStreamAsync(),options);
                foreach (var item in productsubCategory)
                {
                    productSubCategories.Add(new ProductSubCategoryViewModel(item.ID, item.ProductCategoryID, item.Name, item.RowVersion, cryptoParams)
                    {
                        ProductCategory =new ProductCategoryViewModel(item.ProductCategory.ID, item.ProductCategory.Name, item.ProductCategory.RowVersion,cryptoParams),
                        IsDeleted = item.IsDeleted
                    });
                }
            }
            else
            {
                await ErrorMessages(response);
            }
            return View(productSubCategories);
        }

        public async Task<ActionResult> SubCategoryByCategory([ModelBinder(typeof(EncryptDataBinder))] int id, int? pageNo)
        {
            List<ProductSubCategoryViewModel> productSubCategories = new(); 
            int pageNos = pageNo == null ? 1 : (int)pageNo;
            var response = await _httpClient.GetAsync("api/ProductSubCategories/GetProductSubCategories/" + pageNos + "/" + id);

            if (response.IsSuccessStatusCode)
            {
                var productSubCategories22 = await JsonSerializer.DeserializeAsync<List<ProductSubCategory>>(await response.Content.ReadAsStreamAsync(), options); 
                foreach (var item in productSubCategories22)
                {
                    productSubCategories.Add(new ProductSubCategoryViewModel(item.ID, item.ProductCategoryID,item.Name,item.RowVersion,cryptoParams)
                    {
                        ProductCategory = new ProductCategoryViewModel(item.ProductCategory.ID, item.ProductCategory.Name, item.ProductCategory.RowVersion, cryptoParams),
                        IsDeleted = item.IsDeleted
                    });
                }
            }
            else
            {
                await ErrorMessages(response);
            }

            ProductCategory productCategory = new("");
            var response2 = await _httpClient.GetAsync("api/ProductCategories/GetProductCategory/" + id);
            if (response2.IsSuccessStatusCode)
            {
                productCategory = await JsonSerializer.DeserializeAsync<ProductCategory>(await response2.Content.ReadAsStreamAsync(), options);
            }
            else
            {
                await ErrorMessages(response2);
            }
            ViewBag.category = productCategory;
            ProductCategoryViewModel viewModel = new ProductCategoryViewModel(id,"",Array.Empty<byte>(),cryptoParams);
            ViewBag.CategoryID = viewModel.EnID;
            return View(productSubCategories);
        }

        [CryptoValueProvider]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeWithBodyResult((int)HttpStatusCode.BadRequest, "Products Error");
            }

            ProductSubCategoryViewModel productSubCategory = null;
            var response = await _httpClient.GetAsync("api/ProductSubCategories/GetProductSubCategory/" + id);
            if (response.IsSuccessStatusCode)
            {
                var productSubCategoryObj = await JsonSerializer.DeserializeAsync<ProductSubCategory>(await response.Content.ReadAsStreamAsync(), options);
                productSubCategory = new ProductSubCategoryViewModel(productSubCategoryObj.ID, productSubCategoryObj.ProductCategoryID, productSubCategoryObj.Name, productSubCategoryObj.RowVersion, cryptoParams)
                {
                    ProductCategory = new ProductCategoryViewModel(productSubCategoryObj.ProductCategory.ID, productSubCategoryObj.ProductCategory.Name, productSubCategoryObj.ProductCategory.RowVersion, cryptoParams),
                    IsDeleted = productSubCategoryObj.IsDeleted,
                    CreatedDate = productSubCategoryObj.CreatedDate,
                    ModifiedDate = productSubCategoryObj.ModifiedDate
                };
            }
            else
            {
                await ErrorMessages(response);
            }
            if (productSubCategory == null)
            {
                return new NotFoundViewResult("ProductsNotFound");
            }
            ProductCategoryViewModel catModel = new ProductCategoryViewModel(productSubCategory.ProductCategoryID, "", Array.Empty<byte>(), cryptoParams);
            ViewBag.ProductCategoryEnID = catModel.EnID;
            return View(productSubCategory);
        }

        [CryptoValueProvider]
        public async Task<ActionResult> Create(int id)
        {
            List<ProductCategory> ProductCategories = new();
            var response = await _httpClient.GetAsync("api/ProductCategories");
            if (response.IsSuccessStatusCode)
            {
                ProductCategories = await JsonSerializer.DeserializeAsync<List<ProductCategory>>(await response.Content.ReadAsStreamAsync(), options);
            }
            else
            {
                await ErrorMessages(response);
            }
            ViewBag.ProductCategoryID = new SelectList(ProductCategories, "ID", "Name",id);
            ProductCategoryViewModel catModel = new ProductCategoryViewModel(id, "", Array.Empty<byte>(), cryptoParams);
            ViewBag.ProductCategoryEnID = catModel.EnID;
            return View("Create");
        }

        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateProductSubCategory()
        {
            FormValueProvider formValueProvider = new(BindingSource.Form, Request.Form, CultureInfo.InvariantCulture);
            ProductSubCategoryViewModel productSubCategory = new(0, 0, "", Array.Empty<byte>(), cryptoParams)
            {
                ModifiedDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };
            await TryUpdateModelAsync(productSubCategory, "", formValueProvider, p => p.ProductCategoryID, p => p.Name, p => p.RowVersion);

            if (ModelState.IsValid)
            {
                var response = await _httpClient.PostAsJsonAsync("api/ProductSubCategories", productSubCategory);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("SubCategoryByCategory", new { id = EShopHtmlHelper.Encrypt("id=" + productSubCategory.ProductCategoryID.ToString()) });
                }
                else
                {
                    await ErrorMessages(response);
                }
            }
            List<ProductCategory> productCategories = new();
            var response2 = await _httpClient.GetAsync("api/ProductCategories");
            if (response2.IsSuccessStatusCode)
            {
                productCategories = await JsonSerializer.DeserializeAsync<List<ProductCategory>>(await response2.Content.ReadAsStreamAsync(), options);
            }
            else
            {
                await ErrorMessages(response2);
            }
            ViewBag.ProductCategoryID = new SelectList(productCategories, "ID", "Name", productSubCategory.ProductCategoryID);
            return View("Create", productSubCategory);
        }

        [CryptoValueProvider]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeWithBodyResult((int)HttpStatusCode.BadRequest, "Product sub categories Error");
            }
            ProductSubCategoryViewModel productSubCategory = null;
            var response = await _httpClient.GetAsync("api/ProductSubCategories/GetProductSubCategory/" + id);
            if (response.IsSuccessStatusCode)
            {
                var productSubCategoryObj = await JsonSerializer.DeserializeAsync<ProductSubCategory>(await response.Content.ReadAsStreamAsync(), options);
                productSubCategory = new ProductSubCategoryViewModel(productSubCategoryObj.ID, productSubCategoryObj.ProductCategoryID,productSubCategoryObj.Name, productSubCategoryObj.RowVersion, cryptoParams)
                {
                    Name = productSubCategoryObj.Name,
                    ProductCategoryID = productSubCategoryObj.ProductCategoryID,
                    IsDeleted = productSubCategoryObj.IsDeleted,
                    RowVersion = productSubCategoryObj.RowVersion
                };
            }
            else
            {
                await ErrorMessages(response);
            }
            if (productSubCategory == null)
            {
                return new NotFoundViewResult("ProductSubCategoriesNotFoud");
            }
            List<ProductCategory> ProductCategories = new();
            var response2 = await _httpClient.GetAsync("api/ProductCategories");
            if (response2.IsSuccessStatusCode)
            {
                ProductCategories = await JsonSerializer.DeserializeAsync<List<ProductCategory>>(await response2.Content.ReadAsStreamAsync(), options);
            }
            else
            {
                await ErrorMessages(response2);
            }
            ViewBag.ProductCategoryID = new SelectList(ProductCategories, "ID", "Name", productSubCategory.ProductCategoryID);
            Microsoft.AspNetCore.Routing.RouteData routeData = ControllerContext.RouteData;
            ViewBag.encryptedId = routeData.Values["id"];
            ProductCategoryViewModel catModel = new ProductCategoryViewModel(productSubCategory.ProductCategoryID, "", Array.Empty<byte>(), cryptoParams);
            ViewBag.ProductCategoryEnID = catModel.EnID;
            return View(productSubCategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([ModelBinder(typeof(EncryptDataBinder))] int id)
        {
            ProductSubCategory productSubCategory = new(0, "")
            {
                ModifiedDate = DateTime.Now
            };
            FormValueProvider formValProvider = new(BindingSource.Form, HttpContext.Request.Form, CultureInfo.InvariantCulture);
            await TryUpdateModelAsync<ProductSubCategory>(productSubCategory, "", formValProvider, p => p.ID, p => p.Name, p => p.ProductCategoryID, p => p.CreatedDate, p => p.RowVersion);
            TryValidateModel(productSubCategory);

            if (ModelState.IsValid)
            {
                var response = await _httpClient.PutAsJsonAsync("api/ProductSubCategories/PutProductSubCategory/" + productSubCategory.ID, productSubCategory);
                if (response.IsSuccessStatusCode)
                {
                    Dictionary<string, string> pairs = new()
                    {
                        { "id", productSubCategory.ProductCategoryID.ToString() }
                    };
                    return RedirectToAction("SubCategoryByCategory", new { id = cryptoParams.EncryptParamDictionary(pairs) });
                }
                else
                {
                    await ErrorMessages(response);
                }
            }
            Microsoft.AspNetCore.Routing.RouteData routeData = ControllerContext.RouteData;
            ViewBag.encryptedId = routeData.Values["id"];

            List<ProductCategory> ProductCategories = new();
            var response2 = await _httpClient.GetAsync("api/ProductCategories");
            if (response2.IsSuccessStatusCode)
            {
                ProductCategories = await JsonSerializer.DeserializeAsync<List<ProductCategory>>(await response2.Content.ReadAsStreamAsync(), options);
            }
            else
            {
                await ErrorMessages(response2);
            }
            ViewBag.ProductCategoryID = new SelectList(ProductCategories, "ID", "Name", productSubCategory.ProductCategoryID);
            return View("Edit", productSubCategory);
        }

        [CryptoValueProvider]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeWithBodyResult((int)HttpStatusCode.BadRequest, "Product sub categories Error");
            }
            ProductSubCategoryViewModel productSubCategory = null;
            var response = await _httpClient.GetAsync("api/ProductSubCategories/GetProductSubCategory/" + id);
            if (response.IsSuccessStatusCode)
            {
                var productSubCategoryObj = await JsonSerializer.DeserializeAsync<ProductSubCategory>(await response.Content.ReadAsStreamAsync(), options);
                productSubCategory = new ProductSubCategoryViewModel(productSubCategoryObj.ID, productSubCategoryObj.ProductCategoryID, productSubCategoryObj.Name, productSubCategoryObj.RowVersion,cryptoParams)
                {
                    IsDeleted = productSubCategoryObj.IsDeleted,
                    RowVersion = productSubCategoryObj.RowVersion
                };
            }
            else
            {
                await ErrorMessages(response);
            }
            if (productSubCategory == null)
            {
                return new NotFoundViewResult("ProductSubCategoriesNotFoud");
            }
            ProductCategoryViewModel catModel = new ProductCategoryViewModel(productSubCategory.ProductCategoryID, "", Array.Empty<byte>(), cryptoParams);
            ViewBag.ProductCategoryEnID = catModel.EnID;
            return View(productSubCategory);
        }

        [CryptoValueProvider]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ProductSubCategoryViewModel result = null;
            var response = await _httpClient.DeleteAsync("api/ProductSubCategories/DeleteProductSubCategory/" + id);
            if (response.IsSuccessStatusCode)
            {
                Dictionary<string, string> pairs = new()
                {
                    { "id", result.ProductCategoryID.ToString() }
                };
                return RedirectToAction("SubCategoryByCategory", new { id = cryptoParams.EncryptParamDictionary(pairs) });
            }
            else
            {
                await ErrorMessages(response);
                HttpResponseMessage response2 = await _httpClient.GetAsync("api/ProductSubCategories/GetSubCategory/" + id);
                if (response2.IsSuccessStatusCode)
                {
                    var productSubCategoryObj = await JsonSerializer.DeserializeAsync<ProductSubCategory>(await response2.Content.ReadAsStreamAsync(), options);
                    result = new ProductSubCategoryViewModel(productSubCategoryObj.ID, productSubCategoryObj.ProductCategoryID, productSubCategoryObj.Name,productSubCategoryObj.RowVersion,cryptoParams)
                    {
                        IsDeleted = productSubCategoryObj.IsDeleted,
                        RowVersion = productSubCategoryObj.RowVersion
                    };
                }
                else
                {
                    await ErrorMessages(response2);
                }
            }
            return View(result);
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
