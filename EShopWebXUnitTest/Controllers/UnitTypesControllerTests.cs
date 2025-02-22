using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Moq.Protected;
using EShopModels;
using EShopWeb.Common;
using EShopWeb.Filters;
using EShopWeb.Models;
using System.Net.Http.Json;
using EShopWeb.Areas.Admin.Controllers;
using EShopModels.Common;

namespace EShopWeb.Controllers.Tests
{
    public class UnitTypesControllerTests
    {
        private Mock<IHttpContextAccessor> contextAccessor;
        private UnitTypesController unitTypesController; 
        private readonly Mock<IHttpClientFactory> httpClientFactory;
        private Mock<IOptions<EShopSystemConfig>> configure;
        private Mock<ILogger<UnitTypesController>> mocklogger; 
        private Mock<HttpContext> mockHttpContext;
        private readonly Mock<MockHttpMessageHandler> mock;
        private readonly Mock<CryptoParamsProtector> cryptoParams;
        private readonly Mock<IDataProtectionProvider> mockIDataProtectionProvider = new(MockBehavior.Strict);
        private readonly Mock<IDataProtector> mockDpro = new();

        public UnitTypesControllerTests()
        {
            List<UnitTypeViewModel> list = new();
            mockIDataProtectionProvider.Setup(v=>v.CreateProtector(It.IsAny<string>())).Returns(mockDpro.Object);
            cryptoParams = new Mock<CryptoParamsProtector>(MockBehavior.Strict, mockIDataProtectionProvider.Object);
            mock = new Mock<MockHttpMessageHandler>(MockBehavior.Strict);
            string tokenobj;
            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
            {
                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(list), System.Text.Encoding.UTF8, "application/json"),
                StatusCode = System.Net.HttpStatusCode.OK
            }).Verifiable();
            var client = new Mock<HttpClient>(MockBehavior.Strict, mock.Object);
            client.Object.BaseAddress = new Uri("http://localhost:20732");
            httpClientFactory = new Moq.Mock<IHttpClientFactory>();
            httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client.Object);

            LoginView login = new("nalinmyid@keels.com","123Pp[]",false);
            var response = client.Object.PostAsJsonAsync("api/EShopUsers/ValidateToken", login);
            if (response.Result.IsSuccessStatusCode)
            {
                tokenobj = response.Result.Content.ReadAsStringAsync().Result;
            }
        }

        private void InitController()
        {
            byte[] val;
            contextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            Mock<ISession> mocks = new();
            mocks.Setup(c => c.Set("JWToken", It.IsAny<byte[]>())).Callback<string, byte[]>((k, v) => { var tokenobj = v.ToString(); });
            mocks.Setup(v => v.TryGetValue("JWToken", out val)).Returns(true);
            mockHttpContext.Setup(v => v.Session).Returns(mocks.Object);
            contextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);
            configure = new Mock<Microsoft.Extensions.Options.IOptions<EShopSystemConfig>>();
            mocklogger = new Mock<ILogger<UnitTypesController>>();
            unitTypesController = new UnitTypesController(cryptoParams.Object, httpClientFactory.Object, configure.Object, contextAccessor.Object,mocklogger.Object); 
        }

        [Fact()]
        public void IndexTest()
        {
            InitController();
            ViewResult result = unitTypesController.Index(1).Result as ViewResult;
            mock.Verify();
            Assert.IsType<List<UnitTypeViewModel>>(result.ViewData.Model);
        }

        [Fact()]
        public void DetailsTest()
        { 

            InitController();
            var viewResult = unitTypesController.Details(null).Result;
            Assert.IsType<HttpStatusCodeWithBodyResult>(viewResult);

            var viewResult2 = unitTypesController.Details(1).Result;
            mock.Verify();
            Assert.NotNull(viewResult2);
            Assert.NotNull(((ViewResult)viewResult2).ViewData.Model);
            Assert.IsType<ViewResult>(viewResult2); 
        }

        [Fact()]
        public void CreateTest()
        {
            InitController();
            ViewResult result = unitTypesController.Create() as ViewResult;
            Assert.Equal("Create", result.ViewName); 
        }

        [Fact()]
        public void CreateTest1()
        {
            UnitTypeViewModel unitTypeModel = new(1, "Test", "Test", false, Array.Empty<byte>(), cryptoParams.Object)
            {
                CreatedDate = DateTime.Now
            };

            InitController();

            var formdic = new Dictionary<string, StringValues>
            {
                { "Code", "grp0001" },
                { "Name", "Test0012" },
                { "CreatedDate", DateTime.Now.ToString() },
                { "ModifiedDate", DateTime.Now.ToString() },
                { "ID", "1" },
                { "RowVersion", "0000000000000F50" }
            };
            var formValues = new FormCollection(formdic);
            Mock<IRequestCookieCollection> mockrequestcookies = new();
            var request = new Mock<HttpRequest>(MockBehavior.Strict);
            request.Setup(req => req.Method).Returns("POST");
            request.SetupGet(req => req.Form).Returns(formValues); 
            request.Setup(req => req.Cookies).Returns(mockrequestcookies.Object);
            request.Setup(req => req.QueryString).Returns(new QueryString("?ID=1"));
             
            var actionContext = new ActionContext(mockHttpContext.Object, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor());
            unitTypesController.ControllerContext = new ControllerContext(actionContext); 

            var result = unitTypesController.Create();
            mock.Verify();
            if (result is ViewResult)
            {
                Assert.True(unitTypesController.ModelState.IsValid);
            }
            else if (result is RedirectToActionResult result1)
            {
                Assert.Equal("Index", result1.ActionName);
            } 
        }

        [Fact()]
        public void EditTest()
        {
            UnitTypeViewModel unitTypeModel = new(1, "Test", "Test", false, Array.Empty<byte>(), cryptoParams.Object)
            {
                CreatedDate = DateTime.Now
            };
            InitController();
            var request = new Mock<HttpRequest>(MockBehavior.Strict);
            request.Setup(g => g.Method).Returns("POST"); 

            RouteData route = new(); 
            route.Values.Add("action", "Edit");
            route.Values.Add("Id", 1);

            var mockhttpcontext = new Mock<HttpContext>(MockBehavior.Strict);
            mockhttpcontext.Setup(c => c.Request).Returns(request.Object);
            var actionContext = new ActionContext(mockHttpContext.Object, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor());
            unitTypesController.ControllerContext = new ControllerContext(actionContext); 

            var result = unitTypesController.Edit(null).Result;
            Assert.IsType<HttpStatusCodeWithBodyResult>(result);

            ViewResult viewResult = unitTypesController.Edit(1).Result as ViewResult;
            mock.Verify();
            Assert.NotNull(viewResult.ViewData.Model);
            Assert.IsType<UnitTypeViewModel>(viewResult.ViewData.Model); 
        }

        [Fact()]
        public void EditTest1()
        {
            UnitTypeViewModel unitTypeModel = new(1, "Test", "Test", false, Array.Empty<byte>(), cryptoParams.Object)
            {
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                IsDeleted = false,
                RowVersion = new byte[] { 12 }
            };

            InitController();
            var formdic = new Dictionary<string, StringValues>
            {
                { "ID", new StringValues("1") },
                { "Code", unitTypeModel.Code },
                { "CreatedDate", unitTypeModel.CreatedDate.ToShortDateString() },
                { "ModifiedDate", unitTypeModel.ModifiedDate.ToShortDateString() },
                { "Name", unitTypeModel.Name },
                { "IsDeleted", unitTypeModel.IsDeleted.ToString() },
                { "IsBaseUnit", unitTypeModel.IsBaseUnit.ToString() },
                { "RowVersion", unitTypeModel.RowVersion.ToString() }, 
            };

            var form = new FormCollection(formdic);

            var request = new Mock<HttpRequest>(MockBehavior.Strict);
            request.Setup(g => g.Method).Returns("POST");
            request.Setup(v => v.Form).Returns(form);

            mockHttpContext.Setup(g => g.Request).Returns(request.Object);          

            RouteData route = new(); 
            route.Values.Add("action", "Edit");
            route.Values.Add("Id",1);
 
            var actionContext = new ActionContext(mockHttpContext.Object, route, new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor());
            unitTypesController.ControllerContext = new ControllerContext(actionContext); 
            var result = unitTypesController.Edit(1).Result;
            mock.Verify();
            if (result is RedirectToActionResult result1)
            {
                Assert.Equal("Index", result1.ActionName);
            }
            else if (result is ViewResult)
            {
                Assert.True(unitTypesController.ModelState.IsValid);
            } 
        }

        [Fact()]
        public void DeleteTest()
        { 
            InitController();
            var nullidresult = unitTypesController.Delete(null).Result;
            Assert.IsType<HttpStatusCodeWithBodyResult>(nullidresult);

            ViewResult viewResult = unitTypesController.Delete(1).Result as ViewResult;
            mock.Verify();
            Assert.NotNull(viewResult.ViewData.Model);
            Assert.IsType<UnitTypeViewModel>(viewResult.ViewData.Model); 
        }

        [Fact()]
        public void DeleteConfirmedTest()
        { 
            InitController();
            RedirectToActionResult routeResult = unitTypesController.DeleteConfirmed(1).Result as RedirectToActionResult;
            mock.Verify();
            Assert.Null(routeResult);
            Assert.Equal("Index", routeResult.ActionName);
        }
    }
}