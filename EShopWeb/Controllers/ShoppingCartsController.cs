using EShopModels;
using EShopModels.Common;
using EShopWeb.Common;
using EShopWeb.Filters;
using EShopWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace EShopWeb.Controllers
{
    [EShopAuthorize(nameof(Roles.User))]
    public class ShoppingCartsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<EShopSystemConfig> _options;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly CryptoParamsProtector cryptoParams;
        private readonly ILogger<ShoppingCartsController> _logger;
        private readonly JsonSerializerOptions options;
        public ShoppingCartsController(CryptoParamsProtector paramsProtector, IHttpClientFactory httpClientFactory, IOptions<EShopSystemConfig> config, IHttpContextAccessor httpContext, ILogger<ShoppingCartsController> logger)
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

        [HttpGet]
        public async Task<ActionResult> AddToCart()
        {
            return PartialView();
        }

        [HttpPost]
        [CryptoValueProvider]
        public async Task<JsonResult> PostAddToCart([FromBody] AddToCartModel addToCartModel, int id)
        {
            ShoppingCartItem cart = new ShoppingCartItem(0, id, addToCartModel.UnitChartID,0, 0, addToCartModel.Quantity); 
            cart.CreatedDate = DateTime.Now;
            cart.ModifiedDate = DateTime.Now;
            cart.RowVersion = new byte[1];
            string result = ""; 

            if (ModelState.IsValid)
            {
                var response = await _httpClient.PostAsJsonAsync("api/ShoppingCarts", cart);
                if (response.IsSuccessStatusCode)
                {
                    result = "Successfully Saved";
                }
                else
                {
                    result = "Error On Saved";
                    await ErrorMessages(response);
                }
            }
            else
            {
                result = "";
                foreach (var item in ModelState.Values)
                {
                    if (item.Errors.Count > 0)
                    {
                        foreach (var error in item.Errors)
                        {
                            result += error.ErrorMessage;
                        }                       
                    }
                }
            }
            return Json(new { msg = result });
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            ShoppingCartViewModel shoppingCartView = null;
            var response = await _httpClient.GetAsync("api/ShoppingCarts/GetShoppingCartAsync");
            if (response.IsSuccessStatusCode)
            {
                ShoppingCart shoppingCartObj = await JsonSerializer.DeserializeAsync<ShoppingCart>(await response.Content.ReadAsStreamAsync(), options);
                shoppingCartView = new ShoppingCartViewModel(shoppingCartObj.ID, shoppingCartObj.UserID, shoppingCartObj.GrossAmount,shoppingCartObj.DiscountAmount, shoppingCartObj.TaxAmount,cryptoParams)
                {
                    IsDeleted = shoppingCartObj.IsDeleted,
                    CreatedDate = shoppingCartObj.CreatedDate,
                    ModifiedDate = shoppingCartObj.ModifiedDate
                };
                foreach (var item in shoppingCartObj.ShoppingCartItems)
                {
                    shoppingCartView.Items.Add(new ShoppingCartItemViewModel(item.ID, item.ShoppingCartID, item.ProductID, item.UnitChartID, item.UnitPrice, item.LineDiscount, item.Quantity, cryptoParams)
                    {
                        Product = new ProductViewModel(item.Product.ID,0,0,item.Product.Name,"","",0,0,false,0,Array.Empty<byte>(),Array.Empty<byte>(),cryptoParams),
                        UnitChart = new UnitChartViewModel(item.UnitChartID,item.UnitChart.UnitTypeID,item.ProductID,item.UnitChart.Quantity,item.UnitChart.UnitChartName,Array.Empty<byte>(),cryptoParams)
                    });
                }
            }
            else
            {
                await ErrorMessages(response);
            }

            if (shoppingCartView == null)
            {
                return new NotFoundViewResult("ShoppingCartNotFound");
            }
            return View(shoppingCartView);
        }         

        [CryptoValueProvider]
        public async Task<JsonResult> DeleteCartItem(int id)
        {
            ShoppingCartItemViewModel viewModel = null;
            var response = await _httpClient.DeleteAsync("api/ShoppingCarts/DeleteCartItem/" + id);
            if(response.IsSuccessStatusCode)
            {
                return Json(new { msg = "Item Deleted" });
            }
            else
            {
                await ErrorMessages(response);
                return Json(new { msg = "Error on Delete" });
            }
        }

        [HttpPost] 
        [CryptoValueProvider]
        public async Task<ActionResult> UpdateQuantity([FromBody] ShoppingItemQtyUpdate shoppingItem,int id)
        {
            ShoppingCartItem cartItem = new ShoppingCartItem(0, shoppingItem.ProductId, shoppingItem.UnitChartID, 0, 0, shoppingItem.NewQuantity);
            cartItem.RowVersion = new byte[0];
            cartItem.ID = id;
            var response = await _httpClient.PostAsJsonAsync("api/ShoppingCarts/UpdateQuantity", cartItem);
            if (response.IsSuccessStatusCode)
            {
                return Json(new { msg = "Quantity Updated" });
            }
            else
            {
                await ErrorMessages(response);
                return Json(new { msg = "Error on Update" });
            }
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
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                ModelState.AddModelError("", "Service BadRequest");
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
