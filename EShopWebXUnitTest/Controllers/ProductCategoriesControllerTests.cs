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
using System.Net;
using EShopWeb.Areas.Admin.Controllers;
using EShopModels.Common;

namespace EShopWeb.Controllers.Tests
{
    public class ProductCategoriesControllerTests
    {
        private Mock<IHttpContextAccessor> contextAccessor;
        private ProductCategoriesController productCategoriesController;
        private Mock<IHttpClientFactory> httpClientFactory;
        private Mock<IOptions<EShopSystemConfig>> configure;
        private Mock<ILogger<ProductCategoriesController>> mocklogger;
        private Mock<Microsoft.AspNetCore.Http.HttpContext> mockHttpContext;
        private Mock<MockHttpMessageHandler> mock = new(MockBehavior.Strict); 
        private readonly Mock<CryptoParamsProtector> cryptoParams;
 
        public ProductCategoriesControllerTests()
        {
            mock = new Mock<MockHttpMessageHandler>(MockBehavior.Strict);
            cryptoParams = new Mock<CryptoParamsProtector>(MockBehavior.Strict);
        }

        private void InitController(object obj)
        {
            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
            {
                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(obj), System.Text.Encoding.UTF8, "application/json"),
                StatusCode = System.Net.HttpStatusCode.OK
            }).Verifiable();
            var client = new HttpClient(mock.Object)
            {
                BaseAddress = new Uri("http://localhost:20731")
            };
            httpClientFactory = new Moq.Mock<IHttpClientFactory>();
            httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);
            byte[] val = new byte[] { 1 };
            contextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            Mock<ISession> mocks = new();
            mocks.Setup(c => c.Set("JWToken", It.IsAny<byte[]>())).Callback<string, byte[]>((k, v) => { var value = v.ToString(); });
            mocks.Setup(v => v.TryGetValue("JWToken", out val)).Returns(true);
            mockHttpContext.Setup(v => v.Session).Returns(mocks.Object);
            contextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);
            configure = new Mock<Microsoft.Extensions.Options.IOptions<EShopSystemConfig>>();
            mocklogger = new Mock<ILogger<ProductCategoriesController>>();
            productCategoriesController = new ProductCategoriesController(cryptoParams.Object, httpClientFactory.Object, configure.Object, contextAccessor.Object,mocklogger.Object);
        }

        [Fact()]
        public void IndexTest()
        { List<ProductCategoryViewModel> productCategoryViewModels = new();
            InitController(productCategoryViewModels);
            ViewResult result = productCategoriesController.Index(1).Result as ViewResult;
            mock.Verify();
            Assert.IsType<List<ProductCategoryViewModel>>(result.ViewData.Model); 
        }

        [Fact()]
        public void DetailsTest()
        {
            ProductCategoryViewModel productCategoryModel = new(1, "ProductCategory", Array.Empty<byte>(), cryptoParams.Object)
            {
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                RowVersion = Array.Empty<byte>()
            };

            InitController(productCategoryModel);
            var viewResult = productCategoriesController.Details(null).Result;
            Assert.IsType<HttpStatusCodeWithBodyResult>(viewResult);

            var viewResult2 = productCategoriesController.Details(1).Result;
            mock.Verify();
            Assert.NotNull(viewResult2);
            Assert.NotNull(((ViewResult)viewResult2).ViewData.Model);
            Assert.IsType<ViewResult>(viewResult2); 
        }

        

        [Fact()]
        public void CreateProductCategoriesTest()
        {
            ProductCategoryViewModel productCategory = new(1, "cat1", Array.Empty<byte>(), cryptoParams.Object)
            {
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            InitController(productCategory);
            var formdic = new Dictionary<string, StringValues>
            {
                { "Name", "grp0001" },
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
            productCategoriesController.ControllerContext = new ControllerContext(actionContext); 

            var result = productCategoriesController.CreateProductCategories().Result;
            mock.Verify();
            if (result is ViewResult)
            {
                Assert.True(productCategoriesController.ModelState.IsValid);
            }
            else if (result is RedirectToActionResult result1)
            {
                Assert.Equal("Index", result1.ActionName);
            } 
        }

        [Fact()]
        public void EditTest()
        {
            ProductCategoryViewModel productCategoryModel = new(1, "ProductCategory", Array.Empty<byte>(), cryptoParams.Object)
            {
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now 
            };

            InitController(productCategoryModel);
            var request = new Mock<HttpRequest>(MockBehavior.Strict);
            request.Setup(g => g.Method).Returns("POST"); 
            RouteData route = new();
            route.Values.Add("controller", "TaxGroups");
            route.Values.Add("action", "Edit");
            route.Values.Add("Id",1);

            var mockhttpcontext = new Mock<HttpContext>(MockBehavior.Strict);
            mockhttpcontext.Setup(c => c.Request).Returns(request.Object);
            var actionContext = new ActionContext(mockHttpContext.Object, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor());  
            productCategoriesController.ControllerContext = new ControllerContext(actionContext);

            var result = productCategoriesController.Edit(null).Result;
            Assert.IsType<HttpStatusCodeWithBodyResult>(result);

            ViewResult viewResult = productCategoriesController.Edit(1).Result as ViewResult;
            mock.Verify();
            Assert.NotNull(viewResult.ViewData.Model);
            Assert.IsType<ProductCategoryViewModel>(viewResult.ViewData.Model); 
        }

        [Fact()]
        public void EditTest1()
        {
            ProductCategoryViewModel productCategory = new(1, "cat1", Array.Empty<byte>(), cryptoParams.Object)
            {
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            List<EShopUserViewModel> eshopViewUsers = new()
            {
                new EShopUserViewModel(1, "CMD001", "fd", "", "", "", true, Guid.NewGuid(), Roles.User, Array.Empty<byte>(), cryptoParams.Object)
                {
                    Contact = new ContactViewModel(1, 1, "", "", "",Array.Empty<byte>(),cryptoParams.Object),
                    CreatedDate = DateTime.Now,
                    CreditNotes = new List<CreditNoteViewModel>(),
                    IsDeleted = false,
                    ModifiedDate = DateTime.Now,
                    RowVersion = Array.Empty<byte>() 
                }
            };
            List<ProductViewModel> products = new()
            {
                new ProductViewModel(0, 0, 0, "", "", "", 0, 0, false, 0, Array.Empty<byte>(), Array.Empty<byte>(), cryptoParams.Object)
            };

            mock = new Mock<MockHttpMessageHandler>(MockBehavior.Strict);
            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsoluteUri.StartsWith("http://localhost:7223/api/ProductCategories/1")),
                ItExpr.IsAny<CancellationToken>()).ReturnsAsync(
            new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(productCategory), System.Text.Encoding.UTF8, "application/json")
            }).Verifiable();

            HttpClient httpClient = new(mock.Object)
            {
                BaseAddress = new Uri("http://localhost:20731")
            };
            httpClientFactory = new Moq.Mock<IHttpClientFactory>();
            httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            byte[] val = new byte[] { 1 };
            contextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            Mock<ISession> mocks = new();
            mocks.Setup(c => c.Set("JWToken", It.IsAny<byte[]>())).Callback<string, byte[]>((k, v) => { var value = v.ToString(); });
            mocks.Setup(v => v.TryGetValue("JWToken", out val)).Returns(true);
            mockHttpContext.Setup(v => v.Session).Returns(mocks.Object);
            contextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);
            configure = new Mock<Microsoft.Extensions.Options.IOptions<EShopSystemConfig>>();
            mocklogger = new Mock<ILogger<ProductCategoriesController>>();
            productCategoriesController = new ProductCategoriesController(cryptoParams.Object, httpClientFactory.Object, configure.Object, contextAccessor.Object,mocklogger.Object);

            var formdic = new Dictionary<string, StringValues>
            {
                { "Name", "grp0001" },
                { "CreatedDate", DateTime.Now.ToString() },
                { "ModifiedDate", DateTime.Now.ToString() },
                { "ID", "1" },
                { "RowVersion", "0000000000000F50" }
            };
            var formValues = new FormCollection(formdic);

            var request = new Mock<HttpRequest>(MockBehavior.Strict);
            request.Setup(g => g.Method).Returns("POST");
            request.SetupGet(j => j.Form).Returns(formValues); 
            var actionContext = new ActionContext(mockHttpContext.Object, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor());
            productCategoriesController.ControllerContext = new ControllerContext(actionContext); 
            var result = productCategoriesController.Edit(1).Result;
            mock.Verify();
            if (result is RedirectToActionResult result1)
            {
                Assert.Equal("Index", result1.ActionName);
            }
            ///////////////////////////////////////////////////////Model invalid///////////////////////////////////////////////////////////
            formdic["ID"] = "";
            mock = new Mock<MockHttpMessageHandler>(MockBehavior.Strict);

            httpClient = new HttpClient(mock.Object)
            {
                BaseAddress = new Uri("http://localhost:20731")
            };

            httpClientFactory = new Moq.Mock<IHttpClientFactory>();
            httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient); 
            contextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            mocks = new Mock<ISession>();
            mocks.Setup(c => c.Set("JWToken", It.IsAny<byte[]>())).Callback<string, byte[]>((k, v) => { var value = v.ToString(); });
            mocks.Setup(v => v.TryGetValue("JWToken", out val)).Returns(true);
            mockHttpContext.Setup(v => v.Session).Returns(mocks.Object);
            contextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);
            configure = new Mock<Microsoft.Extensions.Options.IOptions<EShopSystemConfig>>();
            mocklogger = new Mock<ILogger<ProductCategoriesController>>();
            productCategoriesController = new ProductCategoriesController(cryptoParams.Object, httpClientFactory.Object, configure.Object, contextAccessor.Object,mocklogger.Object);

            request = new Mock<HttpRequest>(MockBehavior.Strict);
            request.Setup(g => g.Method).Returns("POST");
            request.SetupGet(j => j.Form).Returns(formValues); 
            actionContext = new ActionContext(mockHttpContext.Object, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor());
            productCategoriesController.ControllerContext = new ControllerContext(actionContext); 

            result = productCategoriesController.Edit(1).Result as ViewResult;
            mock.Verify();
            if (result is ViewResult result2)
            {
                Assert.Equal("Edit", result2.ViewName);
            } 
        }

        [Fact()]
        public void DeleteTest()
        {
            ProductCategoryViewModel productCategory = new(1, "1", Array.Empty<byte>(), cryptoParams.Object)
            {
                ModifiedDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };

            InitController(productCategory);
            var nullidresult = productCategoriesController.Delete(null).Result;
            Assert.IsType<HttpStatusCodeWithBodyResult>(nullidresult);

            ViewResult viewResult = productCategoriesController.Delete(1).Result as ViewResult;
            mock.Verify();
            Assert.NotNull(viewResult.ViewData.Model);
            Assert.IsType<ProductCategoryViewModel>(viewResult.ViewData.Model); 
        }

        [Fact()]
        public void DeleteConfirmedTest()
        {
            ProductCategoryViewModel productCategory = new(1, "1", Array.Empty<byte>(), cryptoParams.Object)
            {
                ModifiedDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };

            InitController(productCategory); 
            RedirectToActionResult routeResult = productCategoriesController.DeleteConfirmed(1).Result as RedirectToActionResult;
            mock.Verify();
            Assert.Equal("Index", routeResult.ActionName);
        }
    }
}