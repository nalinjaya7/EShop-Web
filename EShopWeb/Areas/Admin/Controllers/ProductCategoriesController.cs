using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using EShopModels;
using EShopWeb.Common;
using EShopWeb.Filters;
using EShopWeb.Models;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using EShopModels.Common;

namespace EShopWeb.Areas.Admin.Controllers
{
    [EShopAuthorize(nameof(Roles.Admin))]
    [Area("Admin")]
    public class ProductCategoriesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<EShopSystemConfig> _options;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly CryptoParamsProtector cryptoParams;
        private readonly ILogger<ProductCategoriesController> _logger;
        private readonly JsonSerializerOptions options;

        public ProductCategoriesController(CryptoParamsProtector paramsProtector, IHttpClientFactory httpClientFactory, IOptions<EShopSystemConfig> config, IHttpContextAccessor httpContext, ILogger<ProductCategoriesController> logger)
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
            List<ProductCategoryViewModel> productCategories = new();
            int pageNo = pagenumber == null ? 1 : (int)pagenumber;
            var response = await _httpClient.GetAsync("api/ProductCategories/GetProductCategories/" + pageNo);

            if (response.IsSuccessStatusCode)
            {
                var products = await JsonSerializer.DeserializeAsync<List<ProductCategory>>(await response.Content.ReadAsStreamAsync(), options);
                foreach (var item in products)
                {
                    productCategories.Add(new ProductCategoryViewModel(item.ID, item.Name, item.RowVersion, cryptoParams)
                    {
                        IsDeleted = item.IsDeleted
                    });
                }
            }
            else
            {
                await ErrorMessages(response);
            }
            return View(productCategories);
        }

        [CryptoValueProvider]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeWithBodyResult((int)HttpStatusCode.BadRequest, "Product categories Error");
            }
            ProductCategoryViewModel productCategory = null;
            var response = await _httpClient.GetAsync("api/ProductCategories/GetProductCategory/" + id);
            if (response.IsSuccessStatusCode)
            {
                var productCategoryObj = await JsonSerializer.DeserializeAsync<ProductCategory>(await response.Content.ReadAsStreamAsync(), options);
                productCategory = new ProductCategoryViewModel(productCategoryObj.ID, productCategoryObj.Name, productCategoryObj.RowVersion, cryptoParams)
                {
                    CreatedDate = productCategoryObj.CreatedDate,
                    ModifiedDate = productCategoryObj.ModifiedDate,
                    IsDeleted = productCategoryObj.IsDeleted
                };
            }
            else
            {
                await ErrorMessages(response);
            }
            if (productCategory == null)
            {
                return new NotFoundViewResult("ProductCategoriesNotFound");
            }
            return View(productCategory);
        }

        public ActionResult Create()
        {
            return View("Create");
        }

        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateProductCategories()
        {
            FormValueProvider formValueProvider = new(BindingSource.Form, Request.Form, CultureInfo.InvariantCulture);
            ProductCategoryViewModel productCategory = new(0, "",Array.Empty<byte>(),cryptoParams)
            {
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };
            await TryUpdateModelAsync(productCategory, "", formValueProvider, p => p.Name);

            if (ModelState.IsValid)
            {
                var response = await _httpClient.PostAsJsonAsync("api/ProductCategories", productCategory);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    await ErrorMessages(response);
                }
            }

            return View(productCategory);
        }

        [CryptoValueProvider]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeWithBodyResult((int)HttpStatusCode.BadRequest, "Product categories Error");
            }
            ProductCategoryViewModel productCategory = new(0, "",Array.Empty<byte>(),cryptoParams);
            var response = await _httpClient.GetAsync("api/ProductCategories/GetProductCategory/" + id);
            if (response.IsSuccessStatusCode)
            {
               var productCategoryobj = await JsonSerializer.DeserializeAsync<ProductCategory>(await response.Content.ReadAsStreamAsync(), options);
                productCategory = new(productCategoryobj.ID, productCategoryobj.Name, productCategoryobj.RowVersion , cryptoParams);
            }
            else
            {
                await ErrorMessages(response);
            }
            if (productCategory == null)
            {
                return new NotFoundViewResult("ProductCategoriesNotFound");
            }
            Microsoft.AspNetCore.Routing.RouteData routeData = ControllerContext.RouteData;
            ViewBag.encryptedId = routeData.Values["id"];
            return View(productCategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([ModelBinder(typeof(EncryptDataBinder))] int id)
        {
            ModelState.Clear();
            ProductCategory productCategory = new("")
            {
                ModifiedDate = DateTime.Now,
                ID = id
            };
            FormValueProvider formValProvider = new(BindingSource.Form, HttpContext.Request.Form, CultureInfo.InvariantCulture);
            await TryUpdateModelAsync<ProductCategory>(productCategory, "", formValProvider, p => p.ID, p => p.Name, p => p.CreatedDate, p => p.RowVersion);
            TryValidateModel(productCategory);

            if (ModelState.IsValid)
            {
                var response = await _httpClient.PutAsJsonAsync("api/ProductCategories/PutProductCategory/" + productCategory.ID, productCategory);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    await ErrorMessages(response);
                }
            }
            Microsoft.AspNetCore.Routing.RouteData routeData = ControllerContext.RouteData;
            ViewBag.encryptedId = routeData.Values["id"];
            return View("Edit", productCategory);
        }

        [CryptoValueProvider]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeWithBodyResult((int)HttpStatusCode.BadRequest, "Product categories Error");
            }
            ProductCategoryViewModel productCategory=null;
            var response = await _httpClient.GetAsync("api/ProductCategories/GetProductCategory/" + id);
            if (response.IsSuccessStatusCode)
            {
                var productCategoryObj = await JsonSerializer.DeserializeAsync<ProductCategory>(await response.Content.ReadAsStreamAsync(), options);
                productCategory = new(productCategoryObj.ID, "",productCategoryObj.RowVersion,cryptoParams)
                {
                    IsDeleted = productCategoryObj.IsDeleted, 
                    Name = productCategoryObj.Name,
                    CreatedDate = productCategoryObj.CreatedDate,
                    ModifiedDate = productCategoryObj.ModifiedDate
                };
            }
            else
            {
                await ErrorMessages(response);
            }
            if (productCategory == null)
            {
                return new NotFoundViewResult("ProductCategoriesNotFound");
            }
            return View(productCategory);
        }

        [CryptoValueProvider]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ProductCategoryViewModel productCategory = new(id, "", Array.Empty<byte>(), cryptoParams);
            var response = await _httpClient.DeleteAsync("api/ProductCategories/DeleteProductCategory/" + id);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                HttpResponseMessage response2 = await _httpClient.GetAsync("api/ProductCategories/GetProductCategory/" + id);
                if (response2.IsSuccessStatusCode)
                {
                    var productCategoryObj = await JsonSerializer.DeserializeAsync<ProductCategory>(await response2.Content.ReadAsStreamAsync(), options);
                     
                    productCategory.IsDeleted = productCategoryObj.IsDeleted;
                    productCategory.RowVersion = productCategoryObj.RowVersion;
                    productCategory.Name = productCategoryObj.Name;
                    productCategory.CreatedDate = productCategoryObj.CreatedDate;
                    productCategory.ModifiedDate = productCategoryObj.ModifiedDate;
                }
                await ErrorMessages(response);
            }
            return View(productCategory);
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

