using EShopModels;
using EShopModels.Common;
using EShopWeb.Common;
using EShopWeb.Filters;
using EShopWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace EShopWeb.Areas.Admin.Controllers
{
    [EShopAuthorize(nameof(Roles.Admin))]
    [Area("Admin")]
    public class UnitTypesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<EShopSystemConfig> _options;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly CryptoParamsProtector cryptoParams;
        private readonly ILogger<UnitTypesController> _logger;
        private readonly JsonSerializerOptions options;

        public UnitTypesController(CryptoParamsProtector paramsProtector, IHttpClientFactory httpClientFactory, IOptions<EShopSystemConfig> config, IHttpContextAccessor httpContext, ILogger<UnitTypesController> logger)
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
                PropertyNameCaseInsensitive = true
            };
        }

        [CryptoValueProvider]
        public async Task<ActionResult> Index(int? PageNumber)
        {
            List<UnitTypeViewModel> models = new();
            int pagen = (PageNumber == null) ? 1 : (int)PageNumber;

            HttpResponseMessage response = await _httpClient.GetAsync("api/UnitTypes/GetUnitTypes/" + pagen);
            if (response.IsSuccessStatusCode)
            {
                List<UnitType> unitTypes = await JsonSerializer.DeserializeAsync<List<UnitType>>(await response.Content.ReadAsStreamAsync(), options);
                foreach (UnitType item in unitTypes)
                { 
                    models.Add(new UnitTypeViewModel(item.ID, item.Code, item.Name, item.IsBaseUnit, item.RowVersion, cryptoParams)
                    {
                        IsDeleted = item.IsDeleted
                    });
                }
            }
            else
            {
                await ErrorMessages(response);
            }
            return View(models);
        }

        [CryptoValueProvider]
        public async Task<ActionResult> Details(int? id)
        {
            UnitTypeViewModel unitType = null;
            if (id == null)
            {
                return new HttpStatusCodeWithBodyResult((int)HttpStatusCode.BadRequest, "UnitType Error");
            }
            var response = await _httpClient.GetAsync("api/UnitTypes/GetUnitType/" + id);
            if (response.IsSuccessStatusCode)
            {
                var unitTypeObj = await JsonSerializer.DeserializeAsync<UnitType>(await response.Content.ReadAsStreamAsync(), options);
                unitType = new UnitTypeViewModel(unitTypeObj.ID, unitTypeObj.Code, unitTypeObj.Name, unitTypeObj.IsBaseUnit, unitTypeObj.RowVersion, cryptoParams)
                {
                    IsDeleted = unitTypeObj.IsDeleted,
                    CreatedDate = unitTypeObj.CreatedDate,
                    ModifiedDate = unitTypeObj.ModifiedDate
                };
            }
            else
            {
                await ErrorMessages(response);
            }
            if (unitType == null)
            {
                return new NotFoundViewResult("UnitTypesNotFound");
            }
            return View(unitType);
        }

        public ActionResult Create()
        {
            UnitTypeViewModel newunit = new(0, "UNT0001", "", false,Array.Empty<byte>(), cryptoParams);
            return View("Create", newunit);
        }

        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateUnitType()
        {
            FormValueProvider formValueProvider = new(BindingSource.Form, Request.Form, CultureInfo.InvariantCulture);
            UnitTypeViewModel unitType = new(0, "", "", false, Array.Empty<byte>(), cryptoParams)
            {
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };
            await TryUpdateModelAsync(unitType, "", formValueProvider, p => p.Name, p => p.Code, p => p.IsBaseUnit);
            if (ModelState.IsValid)
            {
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/UnitTypes", unitType);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    await ErrorMessages(response);
                }
            }
            return View(unitType);
        }

        [CryptoValueProvider]
        public async Task<ActionResult> Edit(int? id)
        {
            UnitTypeViewModel unitType = null;
            if (id == null)
            {
                return new HttpStatusCodeWithBodyResult((int)HttpStatusCode.BadRequest, "unit type Error");
            }
            var response = await _httpClient.GetAsync("api/UnitTypes/GetUnitType/" + id);
            if (response.IsSuccessStatusCode)
            {
                var unitTypeObj = await JsonSerializer.DeserializeAsync<UnitType>(await response.Content.ReadAsStreamAsync(), options);

                unitType = new UnitTypeViewModel(unitTypeObj.ID, unitTypeObj.Code, unitTypeObj.Name, unitTypeObj.IsBaseUnit, unitTypeObj.RowVersion, cryptoParams)
                {
                    IsDeleted = unitTypeObj.IsDeleted,
                    CreatedDate = unitTypeObj.CreatedDate,
                    ModifiedDate = unitTypeObj.ModifiedDate,
                    RowVersion = unitTypeObj.RowVersion
                };
            }
            else
            {
                await ErrorMessages(response);
            }
            if (unitType == null)
            {
                return new NotFoundViewResult("UnitTypesNotFound");
            }
            Microsoft.AspNetCore.Routing.RouteData routeData = ControllerContext.RouteData;
            ViewBag.encryptedId = routeData.Values["id"];
            return View(unitType);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditUnitType([ModelBinder(typeof(EncryptDataBinder))] int id, IFormCollection formCollection)
        {
            var formValueProvider = new Microsoft.AspNetCore.Mvc.ModelBinding.FormValueProvider(Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource.Form, formCollection, System.Globalization.CultureInfo.CurrentCulture);

            UnitTypeViewModel unitType = new(id, "", "", false, Array.Empty<byte>(), cryptoParams)
            {
                ModifiedDate = DateTime.Now
            };
            FormValueProvider formValProvider = new(BindingSource.Form, HttpContext.Request.Form, CultureInfo.InvariantCulture);
            await TryUpdateModelAsync<UnitTypeViewModel>(unitType, "", formValueProvider, p => p.Code, p => p.Name, p => p.IsBaseUnit, p => p.IsDeleted, p => p.ModifiedDate, p => p.RowVersion, p => p.CreatedDate);
            TryValidateModel(unitType);

            if (ModelState.IsValid)
            {
                var response = await _httpClient.PutAsJsonAsync("api/UnitTypes/PutUnitType/" + id, unitType);
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
            return View(unitType);
        }

        [CryptoValueProvider]
        public async Task<ActionResult> Delete(int? id)
        {
            UnitTypeViewModel unitType = null;
            if (id == null)
            {
                return new HttpStatusCodeWithBodyResult((int)HttpStatusCode.BadRequest, "unit type Error");
            }
            var response = await _httpClient.GetAsync("api/UnitTypes/GetUnitType/" + id);
            if (response.IsSuccessStatusCode)
            {
                var unitTypeObj = await JsonSerializer.DeserializeAsync<UnitType>(await response.Content.ReadAsStreamAsync(), options);

                unitType = new UnitTypeViewModel(unitTypeObj.ID, unitTypeObj.Code, unitTypeObj.Name, unitTypeObj.IsBaseUnit, unitTypeObj.RowVersion, cryptoParams)
                {
                    IsDeleted = unitTypeObj.IsDeleted,
                    CreatedDate = unitTypeObj.CreatedDate,
                    ModifiedDate = unitTypeObj.ModifiedDate 
                };
            }
            else
            {
                await ErrorMessages(response);
            }
            if (unitType == null)
            {
                return new NotFoundViewResult("UnitTypesNotFound");
            }
            return View(unitType);
        }

        [CryptoValueProvider]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            UnitTypeViewModel unitType = null;
            var response = await _httpClient.DeleteAsync("api/UnitTypes/DeleteUnitType/" + id);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                await ErrorMessages(response);
                HttpResponseMessage response2 = await _httpClient.GetAsync("api/UnitTypes/GetUnitType/" + id);
                if (response2.IsSuccessStatusCode)
                {
                    var unitTypeObj = await JsonSerializer.DeserializeAsync<UnitType>(await response2.Content.ReadAsStreamAsync(), options);
                    unitType = new UnitTypeViewModel(unitTypeObj.ID, unitTypeObj.Code, unitTypeObj.Name, unitTypeObj.IsBaseUnit, unitTypeObj.RowVersion , cryptoParams)
                    {
                        IsDeleted = unitTypeObj.IsDeleted,
                        CreatedDate = unitTypeObj.CreatedDate,
                        ModifiedDate = unitTypeObj.ModifiedDate 
                    };
                }
                else
                {
                    await ErrorMessages(response2);
                }
            }
            return View(unitType);
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
