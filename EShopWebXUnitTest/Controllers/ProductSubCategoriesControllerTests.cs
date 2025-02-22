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
    public class ProductSubCategoriesControllerTests
    {
        private Mock<IHttpContextAccessor> contextAccessor;
        private ProductSubCategoriesController productSubCategoriesController; 
        private Mock<IHttpClientFactory> httpClientFactory;
        private Mock<IOptions<EShopSystemConfig>> configure;
        private Mock<ILogger<ProductSubCategoriesController>> mocklogger;
        private Mock<Microsoft.AspNetCore.Http.HttpContext> mockHttpContext;
        private Mock<MockHttpMessageHandler> mock = new(MockBehavior.Strict); 
        private readonly Mock<CryptoParamsProtector> cryptoParams;
       
        public ProductSubCategoriesControllerTests()
        {
            cryptoParams = new Mock<CryptoParamsProtector>(MockBehavior.Strict);
            mock = new Mock<MockHttpMessageHandler>(MockBehavior.Strict);
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
            mocklogger = new Mock<ILogger<ProductSubCategoriesController>>();
            productSubCategoriesController = new ProductSubCategoriesController(cryptoParams.Object, httpClientFactory.Object, configure.Object, contextAccessor.Object,mocklogger.Object);
        }

        [Fact()]
        public void IndexTest()
        {
            List<ProductSubCategoryViewModel> productSubCategories = new();
            InitController(productSubCategories);
            ViewResult result = productSubCategoriesController.Index(1).Result as ViewResult;
            mock.Verify();
            Assert.IsType<List<ProductSubCategoryViewModel>>(result.ViewData.Model); 
        }

        [Fact()]
        public void SubCategoryByCategoryTest()
        {
            ProductCategoryViewModel productCategoryModel = new(1, "test", Array.Empty<byte>(), cryptoParams.Object)
            {
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now 
            };
            List<ProductSubCategoryViewModel> mos = new();
            mock = new Mock<MockHttpMessageHandler>(MockBehavior.Strict);
            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync"
                , ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsoluteUri.StartsWith("http://localhost:7223/api/ProductSubCategories/GetProductSubCategories/1/1"))
                , ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(mos), System.Text.Encoding.UTF8, "application/json")
                }).Verifiable();

            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync"
                , ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsoluteUri.StartsWith("http://localhost:7223/api/ProductCategories/1"))
                , ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(productCategoryModel), System.Text.Encoding.UTF8, "application/json")
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
            mocklogger = new Mock<ILogger<ProductSubCategoriesController>>();
            productSubCategoriesController = new ProductSubCategoriesController(cryptoParams.Object, httpClientFactory.Object, configure.Object, contextAccessor.Object,mocklogger.Object);

            ViewResult result = productSubCategoriesController.SubCategoryByCategory(1, null).Result as ViewResult;
            mock.Verify();
            Assert.IsType<List<ProductSubCategoryViewModel>>(result.ViewData.Model); 
        }

        [Fact()]
        public void DetailsTest()
        {
            ProductSubCategoryViewModel productSubCategoryModel = new(1, 1, "user", Array.Empty<byte>(), cryptoParams.Object)
            {
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                RowVersion = Array.Empty<byte>()
            };

            InitController(productSubCategoryModel);
            var viewResult = productSubCategoriesController.Details(null).Result;
            Assert.IsType<HttpStatusCodeWithBodyResult>(viewResult);

            var viewResult2 = productSubCategoriesController.Details(1).Result;
            mock.Verify();
            Assert.NotNull(viewResult2);
            Assert.NotNull(((ViewResult)viewResult2).ViewData.Model);
            Assert.IsType<ViewResult>(viewResult2); 
        }

        

        [Fact()]
        public void CreateTest1()
        {
            ProductSubCategoryViewModel productSubCategoryModel = new(1, 1, "user", Array.Empty<byte>(), cryptoParams.Object)
            {
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                RowVersion = Array.Empty<byte>()
            };

            InitController("1");
            var formdic = new Dictionary<string, StringValues>
            {
                { "Name", "grp0001" },
                { "CreatedDate", DateTime.Now.ToString() },
                { "ModifiedDate", DateTime.Now.ToString() },
                { "ID", "1" },
                { "ProductCategoryID", "1" },
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
            productSubCategoriesController.ControllerContext = new ControllerContext(actionContext); 

            var result = productSubCategoriesController.Create(1).Result;
            mock.Verify();
            if (result is ViewResult)
            {
                Assert.True(productSubCategoriesController.ModelState.IsValid);
            }
            else if (result is RedirectToActionResult result1)
            {
                Assert.Equal("SubCategoryByCategory", result1.ActionName);
            } 
        }

        [Fact()]
        public void EditTest()
        {
            List<ProductCategoryViewModel> ProductCategories = new();
            ProductSubCategoryViewModel productSubCategoryModel = new(1, 1, "Product SubCategory", Array.Empty<byte>(), cryptoParams.Object)
            {
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now, 
                RowVersion = Array.Empty<byte>()
            };

            mock = new Mock<MockHttpMessageHandler>(MockBehavior.Strict);
            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync"
                , ItExpr.Is<HttpRequestMessage>(ms => ms.RequestUri.AbsoluteUri.StartsWith("http://localhost:7223/api/ProductSubCategories"))
                , ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(productSubCategoryModel), System.Text.Encoding.UTF8, "application/json"),
                    StatusCode = System.Net.HttpStatusCode.OK
                }).Verifiable();

            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync"
            , ItExpr.Is<HttpRequestMessage>(ms => ms.RequestUri.AbsoluteUri.StartsWith("http://localhost:7223/api/ProductCategories"))
            , ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
            {
                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(ProductCategories), System.Text.Encoding.UTF8, "application/json"),
                StatusCode = System.Net.HttpStatusCode.OK
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
            mocklogger = new Mock<ILogger<ProductSubCategoriesController>>();
            productSubCategoriesController = new ProductSubCategoriesController(cryptoParams.Object, httpClientFactory.Object, configure.Object, contextAccessor.Object,mocklogger.Object);
        
            var request = new Mock<HttpRequest>(MockBehavior.Strict);
            request.Setup(g => g.Method).Returns("POST"); 

            RouteData route = new();
            route.Values.Add("controller", "ProductSubCategories");
            route.Values.Add("action", "Edit");
            route.Values.Add("Id",1);
             
            var actionContext = new ActionContext(mockHttpContext.Object, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor());
            productSubCategoriesController.ControllerContext = new ControllerContext(actionContext);

            var result = productSubCategoriesController.Edit(null).Result;
            Assert.IsType<HttpStatusCodeWithBodyResult>(result);

            ViewResult viewResult = productSubCategoriesController.Edit(1).Result as ViewResult;
            mock.Verify();
            Assert.NotNull(viewResult.ViewData.Model);
            Assert.IsType<ProductSubCategoryViewModel>(viewResult.ViewData.Model);
        }

        [Fact()]
        public void EditTest1()
        {
            ProductSubCategoryViewModel productSubCategoryModel = new(1, 1, "user", Array.Empty<byte>(), cryptoParams.Object)
            {
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                RowVersion = Array.Empty<byte>()
            };

            List<EShopUserViewModel> eshopViewUsers = new()
            {
                new EShopUserViewModel(1, "CMD001", "fd", "", "", "", true, Guid.NewGuid(), Roles.User,Array.Empty<byte>(), cryptoParams.Object)
                {
                    Contact = new ContactViewModel(1, 1, "gdf@gdf.jh", "0754524234", "kadawatha",Array.Empty<byte>(),cryptoParams.Object),
                    CreatedDate = DateTime.Now,
                    CreditNotes = new List<CreditNoteViewModel>(),
                    IsDeleted = false,
                    ModifiedDate = DateTime.Now 
                }
            };
            List<ProductViewModel> products = new()
            {
                new ProductViewModel(0, 0, 0, "", "", "", 0, 0, false, 0, Array.Empty<byte>(),Array.Empty<byte>(), cryptoParams.Object)
            };

            mock = new Mock<MockHttpMessageHandler>(MockBehavior.Strict);
            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()).ReturnsAsync(
            new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(productSubCategoryModel), System.Text.Encoding.UTF8, "application/json")
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
            mocklogger = new Mock<ILogger<ProductSubCategoriesController>>();
            productSubCategoriesController = new ProductSubCategoriesController(cryptoParams.Object, httpClientFactory.Object, configure.Object, contextAccessor.Object,mocklogger.Object);

            var formdic = new Dictionary<string, StringValues>
            {
                { "Name", "grp0001" },
                { "CreatedDate", DateTime.Now.ToString() },
                { "ModifiedDate", DateTime.Now.ToString() },
                { "ID", "1" },
                { "ProductCategoryID", "1" },
                { "RowVersion", "0000000000000F50" }
            };

            var formValues = new FormCollection(formdic);

            var request = new Mock<HttpRequest>(MockBehavior.Strict);
            request.Setup(g => g.Method).Returns("POST");
            request.SetupGet(j => j.Form).Returns(formValues); 
            var actionContext = new ActionContext(mockHttpContext.Object, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor());
            productSubCategoriesController.ControllerContext = new ControllerContext(actionContext);
             
            var result = productSubCategoriesController.Edit(1).Result;
            mock.Verify();
            if (result is RedirectToActionResult result1)
            {
                Assert.Equal("SubCategoryByCategory", result1.ActionName);
            }
            ///////////////////////////////////////////////////////Model invalid///////////////////////////////////////////////////////////
            formdic["ID"] = "k";
            List<ProductCategoryViewModel> productCategories = new();
            mock = new Mock<MockHttpMessageHandler>(MockBehavior.Strict);
            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsoluteUri.StartsWith("http://localhost:7223/api/ProductCategories")),
                ItExpr.IsAny<CancellationToken>()).ReturnsAsync(
            new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(productCategories), System.Text.Encoding.UTF8, "application/json")
            }).Verifiable();

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
            mocklogger = new Mock<ILogger<ProductSubCategoriesController>>();
            productSubCategoriesController = new ProductSubCategoriesController(cryptoParams.Object, httpClientFactory.Object, configure.Object, contextAccessor.Object,mocklogger.Object);

            request = new Mock<HttpRequest>(MockBehavior.Strict);
            request.Setup(g => g.Method).Returns("POST");
            request.SetupGet(j => j.Form).Returns(formValues); 
            actionContext = new ActionContext(mockHttpContext.Object, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor());
            productSubCategoriesController.ControllerContext = new ControllerContext(actionContext); 
            result = productSubCategoriesController.Edit(1).Result as ViewResult;
            mock.Verify();
            if (result is ViewResult result2)
            {
                Assert.Equal("Edit", result2.ViewName);
            } 
        }

        [Fact()]
        public void DeleteTest()
        {
            ProductSubCategoryViewModel productSubCategory = new(1, 1, "test", Array.Empty<byte>(), cryptoParams.Object)
            {
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                ProductCategoryID = 1
            };

            InitController(productSubCategory);
            var nullidresult = productSubCategoriesController.Delete(null).Result;
            Assert.IsType<HttpStatusCodeWithBodyResult>(nullidresult);

            ViewResult viewResult = productSubCategoriesController.Delete(1).Result as ViewResult;
            mock.Verify();
            Assert.NotNull(viewResult.ViewData.Model);
            Assert.IsType<ProductSubCategoryViewModel>(viewResult.ViewData.Model); 
        }

        [Fact()]
        public void DeleteConfirmedTest()
        {
            ProductSubCategoryViewModel productSubCategory = new(1, 1, "test", Array.Empty<byte>(), cryptoParams.Object)
            {
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            InitController(productSubCategory);
            RedirectToActionResult routeResult = productSubCategoriesController.DeleteConfirmed(1).Result as RedirectToActionResult;
            mock.Verify();
            Assert.Equal("SubCategoryByCategory", routeResult.ActionName); 
        }
    }
}