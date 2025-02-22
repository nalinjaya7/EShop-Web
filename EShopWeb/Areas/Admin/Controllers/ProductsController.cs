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
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace EShopWeb.Areas.Admin.Controllers
{
    [EShopAuthorize(nameof(Roles.Admin))]
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<EShopSystemConfig> _options;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly CryptoParamsProtector cryptoParams; 
        private readonly ILogger<ProductsController> _logger;
        private readonly JsonSerializerOptions options;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(CryptoParamsProtector paramsProtector, IHttpClientFactory httpClientFactory,
            IOptions<EShopSystemConfig> config, IHttpContextAccessor httpContext, ILogger<ProductsController> logger
           )
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
         
        [HttpPost, HttpGet]
        public async Task<ActionResult> Index(int? pagenumber)
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
            int pageNo = (pagenumber == null) ? 1 : (int)pagenumber;
            var response = await _httpClient.PostAsJsonAsync("api/Products/ProductSearch/" + pageNo, product);

            if (response.IsSuccessStatusCode)
            {
                var products = await JsonSerializer.DeserializeAsync<List<Product>>(await response.Content.ReadAsStreamAsync(), options);
                foreach (Product item in products)
                {
                    models.Add(new ProductViewModel(item.ID, item.ProductSubCategoryID, item.ProductCategoryID, item.Name, item.BarCode, item.ItemCode, 0, 0, false, 0, item.ProductImage, item.RowVersion, cryptoParams)
                    {
                        IsDeleted = item.IsDeleted
                    });
                }
            }
            else
            {
                await ErrorMessages(response);
            }

            List<SelectListItem> ProductSubCategories = new();
            var responses = await _httpClient.GetAsync("api/ProductSubCategories/GetSortedProductSubCategories");
            if (responses.IsSuccessStatusCode)
            {
                var ProductSubCategoriesObj = await JsonSerializer.DeserializeAsync<List<ProductSubCategory>>(await responses.Content.ReadAsStreamAsync(), options);
                foreach (ProductSubCategory itemObj in ProductSubCategoriesObj)
                {
                    ProductSubCategories.Add(new SelectListItem(itemObj.Name, itemObj.ID.ToString(), (product.ProductSubCategoryID == itemObj.ID)));
                }
            }
            else
            {
                await ErrorMessages(responses);
            }
            ProductSubCategories.Insert(0, new SelectListItem("All", "0"));
            ViewData["ProductSubCategoryID"] = ProductSubCategories;
            return View("Index", models);
        }
 
        [HttpGet]
        //[AllowAnonymous]
        public async Task<JsonResult> GetProductBySubCategory(int SubCategory)
        {
            List<ProductViewModel> product = new();
            var response = await _httpClient.GetAsync("api/Products/GetProductsByCategory?SubCategoryID=" + SubCategory);
            if (response.IsSuccessStatusCode)
            {
                var productobj = await JsonSerializer.DeserializeAsync<List<Product>>(await response.Content.ReadAsStreamAsync(), options);
                foreach (Product item in productobj)
                {
                    product.Add(new ProductViewModel(item.ID, SubCategory, item.ProductCategoryID, item.Name, item.BarCode, item.ItemCode, 0, 0, false, 0, item.ProductImage, item.RowVersion, cryptoParams)
                    {
                        IsDeleted = item.IsDeleted
                    });
                }
            }
            else
            {
                await ErrorMessages(response);
            }
            return Json(new SelectList(product, "ID", "Name"));
        }
 
        [CryptoValueProvider]
        //[AllowAnonymous]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeWithBodyResult((int)HttpStatusCode.BadRequest, "Products Error");
            }

            ProductViewModel product = null;
            var response = await _httpClient.GetAsync("api/Products/GetProduct/" + id);
            if (response.IsSuccessStatusCode)
            {
                var productObj = await JsonSerializer.DeserializeAsync<Product>(await response.Content.ReadAsStreamAsync(), options);
                product = new ProductViewModel(productObj.ID, productObj.ProductSubCategoryID, productObj.ProductCategoryID, productObj.Name, productObj.BarCode, productObj.ItemCode, 0, 0, false, 0, productObj.ProductImage, productObj.RowVersion, cryptoParams)
                {
                    CreatedDate = productObj.CreatedDate,
                    ModifiedDate = productObj.ModifiedDate
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

        [CryptoValueProvider]
        public async Task<ActionResult> Create()
        {
            List<ProductCategory> ProductCategories = new();
            List<ProductSubCategory> ProductSubCategories = new();
            var response = await _httpClient.GetAsync("api/ProductCategories");
            if (response.IsSuccessStatusCode)
            {
                ProductCategories = await JsonSerializer.DeserializeAsync<List<ProductCategory>>(await response.Content.ReadAsStreamAsync(), options);
            }
            else
            {
                await ErrorMessages(response);
            }
            ProductCategories.Insert(0, new ProductCategory("<--SELECT Category-->") {});
            ProductSubCategories.Insert(0, new ProductSubCategory(9,"<--SELECT SubCategory-->") { });
            ViewBag.ProductCategoryID = new SelectList(ProductCategories, "ID", "Name");
            ViewBag.ProductSubCategoryID = new SelectList(ProductSubCategories, "ID", "Name", 0);
            ProductViewModel model = new(0,0,0,"", "", "", 0, 0, false,0,Array.Empty<byte>(), Array.Empty<byte>(), cryptoParams);
           
            return View("Create", model);
        }

        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateProduct(IFormFile ProductImage)
        {
            FormValueProvider formValueProvider = new(BindingSource.Form, Request.Form, CultureInfo.InvariantCulture);
            ProductViewModel product = new(0,0, 0, "", "", "", 0, 0, false, 0,Array.Empty<byte>(), Array.Empty<byte>(),cryptoParams);
            await TryUpdateModelAsync<ProductViewModel>(product, "", formValueProvider, p => p.UnitCharts, p => p.ProductImage, p => p.ProductSubCategoryID, p => p.ProductCategoryID, p => p.Name, p => p.BarCode, p => p.ItemCode, p => p.RowVersion, p => p.ReOrderLevel, p => p.TaxInclude, p => p.TaxRate, p => p.TaxGroupID);
            ModelState.Remove("ProductImage");
            product.ProductImage = ConvertImageToByteArray(ProductImage, 100, 100);
            TryValidateModel(product);
            if (ModelState.IsValid)
            {
                product.CreatedDate = DateTime.Now;
                product.ModifiedDate = DateTime.Now;
                var response = await _httpClient.PostAsJsonAsync("api/Products", product);
                if (response.IsSuccessStatusCode)
                {
                    string id = await JsonSerializer.DeserializeAsync<string>(await response.Content.ReadAsStreamAsync(), options);
                    var formFile = product.ProductImage;
                    if (formFile == null || formFile.Length == 0)
                    {
                        ModelState.AddModelError("", "Uploaded file is empty or null.");
                        return View(viewName: "Index");
                    }                

                    return RedirectToAction("Index");
                }
                else
                {
                    await ErrorMessages(response);
                }
            }
            else
            {
                List<ProductCategory> ProductCategories = new();
                List<ProductSubCategory> ProductSubCategories = new();
                var responsea = await _httpClient.GetAsync("api/ProductCategories");
                if (responsea.IsSuccessStatusCode)
                {
                    ProductCategories = await JsonSerializer.DeserializeAsync<List<ProductCategory>>(await responsea.Content.ReadAsStreamAsync(), options);
                }
                else
                {
                    await ErrorMessages(responsea);
                }
                ProductCategories.Insert(0, new ProductCategory("<--SELECT ProductCategory-->") { ID = 0 });
                ViewBag.ProductCategoryID = new SelectList(ProductCategories, "ID", "Name", product.ProductCategoryID);
                if (product.ProductCategoryID > 0)
                {
                    var responsesub = await _httpClient.GetAsync("api/ProductSubCategories/GetSubCategoriesByCategory/" + product.ProductCategoryID);
                    if (responsesub.IsSuccessStatusCode)
                    {
                        ProductSubCategories = await JsonSerializer.DeserializeAsync<List<ProductSubCategory>>(await responsesub.Content.ReadAsStreamAsync(), options);
                    }
                    ProductSubCategories.Insert(0, new ProductSubCategory(product.ProductCategoryID, "<--SELECT ProductSubCategory-->") { ID = 0 });
                    ViewBag.ProductSubCategoryID = new SelectList(ProductSubCategories, "ID", "Name", product.ProductSubCategoryID);
                }
                {
                    ProductSubCategories.Insert(0, new ProductSubCategory(product.ProductCategoryID, "<--SELECT SubCategory-->") { ID = 0 });
                    ViewBag.ProductSubCategoryID = new SelectList(ProductSubCategories, "ID", "Name", product.ProductSubCategoryID);
                }
       
            }
            return View("Create", product);
        }

        [CryptoValueProvider]
        public async Task<ActionResult> Edit(int? id)
        {            
            if (id == null)
            {
                return new HttpStatusCodeWithBodyResult((int)HttpStatusCode.BadRequest, "Products Error");
            }
            ProductViewModel product = null;
            var response = await _httpClient.GetAsync("api/Products/GetProduct/" + id);
            if (response.IsSuccessStatusCode)
            {
                var productObj = await JsonSerializer.DeserializeAsync<Product>(await response.Content.ReadAsStreamAsync(), options);
                product = new ProductViewModel(productObj.ID, productObj.ProductSubCategoryID, productObj.ProductCategoryID, productObj.Name, productObj.BarCode, productObj.ItemCode, productObj.ReOrderLevel, productObj.TaxGroupID, productObj.TaxInclude, 0, productObj.ProductImage, productObj.RowVersion, cryptoParams)
                {
                    CreatedDate = productObj.CreatedDate, 
                    ProductImage = productObj.ProductImage,
                    UnitCharts = new List<UnitChartViewModel>()
                };
                productObj.UnitCharts.ForEach(delegate (UnitChart uc)
                { 
                    product.UnitCharts.Add(new UnitChartViewModel(uc.ID, uc.UnitTypeID, uc.ProductID, uc.Quantity, uc.UnitChartName,uc.RowVersion,cryptoParams)
                    {
                        CreatedDate = uc.CreatedDate,
                        IsDeleted = uc.IsDeleted,
                        ModifiedDate = uc.ModifiedDate, 
                        UnitType = new UnitTypeViewModel(uc.UnitType.ID, uc.UnitType.Code, uc.UnitType.Name, false, uc.RowVersion, cryptoParams) { }
                    });
                });
                product.ProductCategoryID = productObj.ProductCategoryID;
                product.ProductSubCategoryID = productObj.ProductSubCategoryID;
            }
            else
            {
                await ErrorMessages(response);
            }
            if (product == null)
            {
                return new NotFoundViewResult("ProductsNotFound");
            }
            List<ProductCategory> ProductCategories = new();
            var responses = await _httpClient.GetAsync("api/ProductCategories/GetAllProductCategories");
            if (responses.IsSuccessStatusCode)
            {
                ProductCategories = await JsonSerializer.DeserializeAsync<List<ProductCategory>>(await responses.Content.ReadAsStreamAsync(), options);
            }
            else
            {
                await ErrorMessages(responses);
            }
            ViewBag.ProductCategoryID = new SelectList(ProductCategories, "ID", "Name");
            List<ProductSubCategory> ProductSubCategories = new();
            if (product.ProductSubCategoryID > 0)
            {
                var responsesa = await _httpClient.GetAsync("api/ProductSubCategories/GetSubCategoriesByCategory/" + product.ProductCategoryID);
                if (responsesa.IsSuccessStatusCode)
                {
                    var listPaginated = await JsonSerializer.DeserializeAsync<List<ProductSubCategory>>(await responsesa.Content.ReadAsStreamAsync(), options);
                    foreach (ProductSubCategory item in listPaginated)
                    {
                        ProductSubCategories.Add(new ProductSubCategory(product.ProductCategoryID, item.Name)
                        {

                        });
                    }
                }
                else
                {
                    await ErrorMessages(responsesa);
                }
            }

            ViewBag.ProductSubCategoryID = new SelectList(ProductSubCategories, "ID", "Name");
            Microsoft.AspNetCore.Routing.RouteData routeData = ControllerContext.RouteData;
            ViewBag.encryptedId = routeData.Values["id"];             
            return View("Edit", product);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetProductSubCategories(int CategoryId)
        {
            List<ProductSubCategoryViewModel> ProductSubCategories = new();
            var responsea = await _httpClient.GetAsync("api/ProductSubCategories/GetSubCategoriesByCategory/" + CategoryId);
            if (responsea.IsSuccessStatusCode)
            {
                List<ProductSubCategory> listPaginated = await JsonSerializer.DeserializeAsync<List<ProductSubCategory>>(await responsea.Content.ReadAsStreamAsync(), options);
                foreach (ProductSubCategory item in listPaginated)
                {
                    ProductSubCategories.Add(new ProductSubCategoryViewModel(item.ID, CategoryId, item.Name, item.RowVersion, cryptoParams)
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
        public async Task<JsonResult> GetUnitChartsByProduct(int ProductID)
        {
            List<UnitChart> unitChartModels = new();
            var responsea = await _httpClient.GetAsync("api/UnitCharts/GetUnitChartsByProduct/" + ProductID);
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
        public async Task<JsonResult> UpdateUnitChart(int ProductID, [FromBody] UnitChart[] unitChartModel)
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
        public async Task<JsonResult> DeleteUnitChart(int UnitchartID, int ProductID)
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

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditProduct([ModelBinder(typeof(EncryptDataBinder))] int id, IFormFile ProductImage)
        {
            FormValueProvider formValProvider = new(BindingSource.Form, HttpContext.Request.Form, CultureInfo.InvariantCulture);
            List<UnitChartViewModel> unitCharts = new();
            ProductViewModel product = new(id, 0, 0, "", "", "", 0, 0, false, 0, Array.Empty<byte>(), Array.Empty<byte>(), cryptoParams)
            {
                ModifiedDate = DateTime.Now,
                IsDeleted = false
            };
            IDictionary<string, string> keyValuePairs = formValProvider.GetKeysFromPrefix("UnitCharts");
            for (int i = 0; i < keyValuePairs.Count; i++)
            {
                unitCharts.Add(new UnitChartViewModel(0, Convert.ToInt32(HttpContext.Request.Form["UnitCharts[" + i + "].UnitTypeID"]), Convert.ToInt32(HttpContext.Request.Form["UnitCharts[" + i + "].ProductID"]), Convert.ToInt32(HttpContext.Request.Form["UnitCharts[" + i + "].Quantity"]), HttpContext.Request.Form["UnitCharts[" + i + "].UnitChartName"],Array.Empty<byte>(), cryptoParams)
                {

                });
            }

            product.UnitCharts = unitCharts;
            await TryUpdateModelAsync<ProductViewModel>(product, "", formValProvider, p => p.Name, p => p.ProductSubCategoryID, p => p.ProductCategoryID,
                p => p.CreatedDate, p => p.BarCode, p => p.RowVersion, p => p.ItemCode, p => p.ReOrderLevel, p => p.TaxGroupID, p => p.TaxInclude, p => p.TaxRate);

           
            ModelState.Remove("ProductImage");
            product.ProductImage = ConvertImageToByteArray(ProductImage, 100, 100);
            TryValidateModel(product);
            if (ModelState.IsValid)
            {               
                HttpResponseMessage response = await _httpClient.PutAsJsonAsync("api/Products/PutProduct/" + id, product);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    await ErrorMessages(response);
                }
            }
            else
            {
                Microsoft.AspNetCore.Routing.RouteData routeData = ControllerContext.RouteData;
                ViewBag.encryptedId = routeData.Values["id"];

                List<ProductCategory> ProductCategories = new();
                var responses = await _httpClient.GetAsync("api/ProductCategories/GetAllProductCategories");
                if (responses.IsSuccessStatusCode)
                {
                    ProductCategories = await JsonSerializer.DeserializeAsync<List<ProductCategory>>(await responses.Content.ReadAsStreamAsync(), options);
                }
                else
                {
                    await ErrorMessages(responses);
                }

                ViewBag.ProductCategoryID = new SelectList(ProductCategories, "ID", "Name");

                List<SelectListItem> ProductSubCategories = new();
                if (product.ProductSubCategoryID > 0)
                {
                    var responsesa = await _httpClient.GetAsync("api/ProductSubCategories/GetSubCategoriesByCategory/" + product.ProductCategoryID);
                    if (responsesa.IsSuccessStatusCode)
                    {
                        var listPaginated = await JsonSerializer.DeserializeAsync<List<ProductSubCategory>>(await responsesa.Content.ReadAsStreamAsync(), options);
                        foreach (ProductSubCategory item in listPaginated)
                        {
                            ProductSubCategories.Add(new SelectListItem(item.Name, product.ProductSubCategoryID.ToString()));
                        }
                    }
                    else
                    {
                        await ErrorMessages(responsesa);
                    }
                }

                ViewData["ProductSubCategoryID"] = new SelectList(ProductSubCategories, "Value", "Text");
 
                var responsePrj = await _httpClient.GetAsync("api/Products/GetProduct/" + id);
                if (responsePrj.IsSuccessStatusCode)
                {
                    var productObj = await JsonSerializer.DeserializeAsync<Product>(await responsePrj.Content.ReadAsStreamAsync(), options);
                    product.ProductImage = productObj.ProductImage;
                }
            }
            return View("Edit", product);
        }
   
        [CryptoValueProvider]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeWithBodyResult((int)HttpStatusCode.BadRequest, "Products Error");
            }
            ProductViewModel product = null;
            var response = await _httpClient.GetAsync("api/Products/GetProduct/" + id);
            if (response.IsSuccessStatusCode)
            {
                var productObj = await JsonSerializer.DeserializeAsync<Product>(await response.Content.ReadAsStreamAsync(), options);
                product = new ProductViewModel(productObj.ID, productObj.ProductSubCategoryID, productObj.ProductCategoryID, productObj.Name, productObj.BarCode, productObj.ItemCode, productObj.ReOrderLevel, productObj.TaxGroupID, productObj.TaxInclude, 0, productObj.ProductImage, productObj.RowVersion, cryptoParams)
                {
                    CreatedDate = productObj.CreatedDate, 
                    IsDeleted = productObj.IsDeleted
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

        [CryptoValueProvider]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ProductViewModel product = null;
            var response = await _httpClient.DeleteAsync("api/Products/DeleteProduct/" + id);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                HttpResponseMessage response2 = await _httpClient.GetAsync("api/Products/GetProduct/" + id);
                if (response2.IsSuccessStatusCode)
                {
                    var productObj = await JsonSerializer.DeserializeAsync<Product>(await response2.Content.ReadAsStreamAsync(), options);
                    product = new ProductViewModel(productObj.ID, productObj.ProductSubCategoryID, productObj.ProductCategoryID, productObj.Name, productObj.BarCode, productObj.ItemCode, productObj.ReOrderLevel, productObj.TaxGroupID, productObj.TaxInclude, 0, productObj.ProductImage, productObj.RowVersion, cryptoParams)
                    {
                        CreatedDate = productObj.CreatedDate, 
                        IsDeleted = productObj.IsDeleted
                    };
                }
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
    
        public Byte[] ConvertImageToByteArray(IFormFile ProductImage, int Width,int Height)
        {
            using var memoryStream = new MemoryStream();
            if (ProductImage != null)
            {
                using (Image img = Image.FromStream(ProductImage.OpenReadStream(), false, true))
                {
                    Bitmap newImage = new(Width, Height);
                    using (Graphics g = Graphics.FromImage(newImage))
                    {
                        g.DrawImage(img, 0, 0, Width, Height);
                    }
                    newImage.Save(memoryStream, ImageFormat.Jpeg);
                }
                return memoryStream.ToArray();
            }
            else
            {
                return null;
            }
        }
    }
}
