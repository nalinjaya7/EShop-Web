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
    public class ProductsControllerTests
    {
        private Mock<IHttpContextAccessor> contextAccessor;
        private ProductsController productsController;
        private Areas.Admin.Controllers.ProductsController productsController_Admin;
        private Mock<IHttpClientFactory> httpClientFactory;
        private Mock<IOptions<EShopSystemConfig>> configure;
        private Mock<ILogger<ProductsController>> mocklogger;
        private Mock<ILogger<Areas.Admin.Controllers.ProductsController>> mocklogger_Admin;
        private Mock<Microsoft.AspNetCore.Http.HttpContext> mockHttpContext;
        private Mock<MockHttpMessageHandler> mock = new(MockBehavior.Strict); 
        private readonly Mock<CryptoParamsProtector> cryptoParams;
    
        public ProductsControllerTests()
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
            mocklogger = new Mock<ILogger<ProductsController>>();
            mocklogger_Admin = new Mock<ILogger<Areas.Admin.Controllers.ProductsController>>();
            productsController = new ProductsController(cryptoParams.Object, httpClientFactory.Object, configure.Object, contextAccessor.Object,mocklogger.Object);
            productsController_Admin = new Areas.Admin.Controllers.ProductsController(cryptoParams.Object, httpClientFactory.Object, configure.Object, contextAccessor.Object, mocklogger_Admin.Object);
        } 

        [Fact()]
        public void SearchTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void GetProductBySubCategoryTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void DetailsTest()
        {
            ProductViewModel Product = new(0, 0, 1, "GRP0001", "bg", "kj", 0, 0, false, 0, Array.Empty<byte>(), Array.Empty<byte>(), cryptoParams.Object)
            {
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now 
            };

            InitController(Product);
            var viewResult = productsController.Details(null).Result;
            Assert.IsType<HttpStatusCodeWithBodyResult>(viewResult);

            var viewResult2 = productsController.Details(1).Result;
            mock.Verify();
            Assert.NotNull(viewResult2);
            Assert.NotNull(((ViewResult)viewResult2).ViewData.Model);
            Assert.IsType<ViewResult>(viewResult2); 
        }

        [Fact()]
        public void CreateTest()
        {
            List<TaxGroupViewModel> taxGroupModels = new();
            List<ProductCategoryViewModel> productCategoryModels = new();
            mock = new Mock<MockHttpMessageHandler>(MockBehavior.Strict);
            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync"
                , ItExpr.Is<HttpRequestMessage>(ms => ms.RequestUri.AbsoluteUri.StartsWith("http://localhost:7223/api/ProductCategories"))
                , ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(productCategoryModels), System.Text.Encoding.UTF8, "application/json"),
                    StatusCode = System.Net.HttpStatusCode.OK
                }).Verifiable();

            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync"
                , ItExpr.Is<HttpRequestMessage>(n => n.RequestUri.AbsoluteUri.StartsWith("http://localhost:7223/api/TaxGroups/GetTaxGroups"))
                , ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(taxGroupModels), System.Text.Encoding.UTF8, "application/json"),
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
            mocklogger_Admin = new Mock<ILogger<Areas.Admin.Controllers.ProductsController>>();
            productsController_Admin = new Areas.Admin.Controllers.ProductsController(cryptoParams.Object, httpClientFactory.Object, configure.Object, contextAccessor.Object,mocklogger_Admin.Object);

            ViewResult result = productsController_Admin.Create().Result as ViewResult;
            Assert.Equal("Create", result.ViewName); 
        }

        [Fact()]
        public void CreateTest1()
        {
            ProductViewModel Product = new(0, 0, 0, "GRP0001", "bg", "kj", 0, 0, false, 0, Array.Empty<byte>(), Array.Empty<byte>(), cryptoParams.Object)
            {
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now, 
                UnitCharts = new List<UnitChartViewModel>
            {
                new(1, 1, 1, 1, "EA",Array.Empty<byte>(),cryptoParams.Object)
                {
                    CreatedDate = DateTime.Now,
                    IsDeleted = false,
                    ModifiedDate = DateTime.Now,
                    RowVersion = Array.Empty<byte>()
                }
            }
            };

            mock = new Mock<MockHttpMessageHandler>(MockBehavior.Strict);
            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
            {
                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject("1"), System.Text.Encoding.UTF8, "application/json"),
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
            mocklogger_Admin = new Mock<ILogger<Areas.Admin.Controllers.ProductsController>>();
            productsController_Admin = new Areas.Admin.Controllers.ProductsController(cryptoParams.Object, httpClientFactory.Object, configure.Object, contextAccessor.Object,mocklogger_Admin.Object);

            var formdic = new Dictionary<string, StringValues>
            {
                { "Name", "grp0001" },
                { "CreatedDate", DateTime.Now.ToString() },
                { "ModifiedDate", DateTime.Now.ToString() },
                { "ID", "1" },
                { "RowVersion", "0000000000000F50" },
                { "ProductSubCategoryID", "1" },
                { "ProductCategoryID", "1" },
                { "BarCode", "1" },
                { "ItemCode", "1" },
                { "ReOrderLevel", "1" },
                { "TaxInclude", "true" },
                { "TaxRate", "1" },
                { "TaxRate", "1" },
                { "UnitCharts[0].CreatedDate", DateTime.Now.ToShortDateString() },
                { "UnitCharts[0].ID", "1" },
                { "UnitCharts[0].IsDeleted", "false" },
                { "UnitCharts[0].ModifiedDate", DateTime.Now.ToShortDateString() }, 
                { "UnitCharts[0].ProductID", "1" },
                { "UnitCharts[0].Quantity", "1" },
                { "UnitCharts[0].RowVersion", "0000000000000F50" },
                { "UnitCharts[0].UnitChartName", "1" },
                { "UnitCharts[0].UnitTypeID", "1" }
            };

            var formValues = new FormCollection(formdic);
            Mock<IRequestCookieCollection> mockrequestcookies = new();
            var request = new Mock<HttpRequest>(MockBehavior.Strict);
            request.Setup(req => req.Method).Returns("POST");
            request.SetupGet(req => req.Form).Returns(formValues); 
            request.Setup(req => req.Cookies).Returns(mockrequestcookies.Object);
            request.Setup(req => req.QueryString).Returns(new QueryString("?ID=1"));
             
            var actionContext = new ActionContext(mockHttpContext.Object, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor());
            productsController_Admin.ControllerContext = new ControllerContext(actionContext); 

            var result = productsController_Admin.Create().Result;
            mock.Verify();
            if (result is ViewResult)
            {
                Assert.True(productsController.ModelState.IsValid);
            }
            else if (result is RedirectToActionResult result1)
            {
                Assert.Equal("Index", result1.ActionName);
            }
            //////////////////////////////////////////////////////////////////////////////////////////////
            formdic["Name"] = "";
            List<ProductCategoryViewModel> ProductCategories = new();
            List<ProductSubCategoryViewModel> ProductSubCategories = new();
            List<TaxGroupViewModel> taxGroupModels = new();
            mock = new Mock<MockHttpMessageHandler>(MockBehavior.Strict);
            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsoluteUri.StartsWith("http://localhost:7223/api/ProductCategories")),
                ItExpr.IsAny<CancellationToken>()).ReturnsAsync(
            new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(ProductCategories), System.Text.Encoding.UTF8, "application/json")
            }).Verifiable();

            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsoluteUri.StartsWith("http://localhost:7223/api/ProductSubCategories/GetSubCategoriesByCategory/1")),
                ItExpr.IsAny<CancellationToken>()).ReturnsAsync(
            new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(ProductSubCategories), System.Text.Encoding.UTF8, "application/json")
            }).Verifiable();

            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsoluteUri.StartsWith("http://localhost:7223/api/TaxGroups/GetTaxGroups")),
                ItExpr.IsAny<CancellationToken>()).ReturnsAsync(
            new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(taxGroupModels), System.Text.Encoding.UTF8, "application/json")
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
            mocklogger_Admin = new Mock<ILogger<Areas.Admin.Controllers.ProductsController>>();
            productsController_Admin = new Areas.Admin.Controllers.ProductsController(cryptoParams.Object, httpClientFactory.Object, configure.Object, contextAccessor.Object,mocklogger_Admin.Object);

            request = new Mock<HttpRequest>(MockBehavior.Strict);
            request.Setup(req => req.Method).Returns("POST");
            request.SetupGet(req => req.Form).Returns(formValues); 
            request.Setup(req => req.Cookies).Returns(mockrequestcookies.Object);
            request.Setup(req => req.QueryString).Returns(new QueryString("?ID=1")); 
            actionContext = new ActionContext(mockHttpContext.Object, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor());
            productsController_Admin.ControllerContext = new ControllerContext(actionContext); 
            result = productsController_Admin.Create().Result;
            mock.Verify();
            if (result is ViewResult)
            {
                Assert.True(productsController.ModelState.IsValid);
            } 
        }

        [Fact()]
        public void EditTest()
        {
            ProductViewModel product = new(0, 0, 1, "test", "", "", 0, 0, false, 0, Array.Empty<byte>(), Array.Empty<byte>(), cryptoParams.Object)
            {
                ProductCategoryID = 1,
                ProductSubCategoryID = 1
            };
            List<ProductCategoryViewModel> ProductCategories = new()
            {
                new ProductCategoryViewModel(1, "",Array.Empty<byte>(), cryptoParams.Object) { }
            };
            List<ProductSubCategoryViewModel> ProductSubCategories = new();
            List<TaxGroupViewModel> taxGroupModels = new();

            mock = new Mock<MockHttpMessageHandler>(MockBehavior.Strict);
            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync"
                , ItExpr.Is<HttpRequestMessage>(ms => ms.RequestUri.AbsoluteUri.StartsWith("http://localhost:7223/api/Products/GetProduct/1/1"))
                , ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(product), System.Text.Encoding.UTF8, "application/json"),
                    StatusCode = System.Net.HttpStatusCode.OK
                }).Verifiable();

            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync"
                , ItExpr.Is<HttpRequestMessage>(n => n.RequestUri.AbsoluteUri.StartsWith("http://localhost:7223/api/ProductCategories"))
                , ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(ProductCategories), System.Text.Encoding.UTF8, "application/json"),
                    StatusCode = System.Net.HttpStatusCode.OK
                }).Verifiable();

            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync"
            , ItExpr.Is<HttpRequestMessage>(n => n.RequestUri.AbsoluteUri.StartsWith("http://localhost:7223/api/ProductSubCategories/GetSubCategoriesByCategory/1"))
            , ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
            {
                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(ProductSubCategories), System.Text.Encoding.UTF8, "application/json"),
                StatusCode = System.Net.HttpStatusCode.OK
            }).Verifiable();

            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync"
            , ItExpr.Is<HttpRequestMessage>(n => n.RequestUri.AbsoluteUri.StartsWith("http://localhost:7223/api/TaxGroups/GetTaxGroups"))
            , ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
            {
                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(taxGroupModels), System.Text.Encoding.UTF8, "application/json"),
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
            Mock<ILogger<EShopWeb.Areas.Admin.Controllers.ProductsController>> mocklogger2 = new();
            EShopWeb.Areas.Admin.Controllers.ProductsController productsController2 = new(cryptoParams.Object, httpClientFactory.Object, configure.Object,contextAccessor.Object,mocklogger2.Object);

            var request = new Mock<HttpRequest>(MockBehavior.Strict);
            request.Setup(g => g.Method).Returns("POST"); 

            RouteData route = new();
            route.Values.Add("controller", "Products");
            route.Values.Add("action", "Edit");
            route.Values.Add("Id", 1); 
            var actionContext = new ActionContext(mockHttpContext.Object, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor());
            productsController2.ControllerContext = new ControllerContext(actionContext);
            var result = productsController2.Edit(null).Result as ViewResult;
            Assert.IsType<HttpStatusCodeWithBodyResult>(result);

            result = productsController2.Edit(1).Result as ViewResult;
            mock.Verify();
            Assert.Equal("Edit", result.ViewName); 
        }

        [Fact()]
        public void GetProductSubCategoriesTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void GetUnitTypesTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void GetUnitChartsByProductTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void UpdateUnitChartTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void DeleteUnitChartTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void EditTest1()
        {
            ProductViewModel Product = new(1, 1, 1, "GRP0001", "bg", "kj", 1, 0, false, 0, Array.Empty<byte>(), Array.Empty<byte>(), cryptoParams.Object)
            {
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                UnitCharts = new List<UnitChartViewModel>
            {
                new(1, 1, 1, 1, "EA",Array.Empty<byte>(), cryptoParams.Object)
                {
                    CreatedDate = DateTime.Now,
                    IsDeleted = false,
                    ModifiedDate = DateTime.Now
                }
            }
            };

            List<EShopUserViewModel> eshopViewUsers = new()
            {
                new EShopUserViewModel(1, "CMD001", "fd", "", "", "", true, Guid.NewGuid(), Roles.User,Array.Empty<byte>(), cryptoParams.Object)
                {
                    Contact = new ContactViewModel(1, 1, "", "", "",Array.Empty<byte>(),cryptoParams.Object),
                    CreatedDate = DateTime.Now,
                    CreditNotes = new List<CreditNoteViewModel>(),
                    IsDeleted = false,
                    ModifiedDate = DateTime.Now 
                }
            };
            List<ProductViewModel> products = new()
            {
                new ProductViewModel(0, 0, 0, "", "", "", 1, 0, false, 0, Array.Empty<byte>(),Array.Empty<byte>(), cryptoParams.Object)
            };

            mock = new Mock<MockHttpMessageHandler>(MockBehavior.Strict);
            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()).ReturnsAsync(
            new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(Product), System.Text.Encoding.UTF8, "application/json")
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
            Mock<ILogger<EShopWeb.Areas.Admin.Controllers.ProductsController>> mocklogger2 = new();
            EShopWeb.Areas.Admin.Controllers.ProductsController productsController2 = new(cryptoParams.Object, httpClientFactory.Object, configure.Object, contextAccessor.Object,mocklogger2.Object);

            var formdic = new Dictionary<string, StringValues>
            {
                { "Name", "grp0001" },
                { "CreatedDate", DateTime.Now.ToString() },
                { "ModifiedDate", DateTime.Now.ToString() },
                { "ID", "1" },
                { "RowVersion", "0000000000000F50" },
                { "ProductSubCategoryID", "1" },
                { "ProductCategoryID", "1" },
                { "BarCode", "1" },
                { "ItemCode", "1" },
                { "ReOrderLevel", "1" },
                { "TaxInclude", "true" },
                { "TaxRate", "1" },
                { "TaxRate", "1" },
                { "UnitCharts[0].CreatedDate", DateTime.Now.ToShortDateString() },
                { "UnitCharts[0].ID", "1" },
                { "UnitCharts[0].IsDeleted", "false" },
                { "UnitCharts[0].ModifiedDate", DateTime.Now.ToShortDateString() }, 
                { "UnitCharts[0].ProductID", "1" },
                { "UnitCharts[0].Quantity", "1" },
                { "UnitCharts[0].RowVersion", "0000000000000F50" },
                { "UnitCharts[0].UnitChartName", "1" },
                { "UnitCharts[0].UnitTypeID", "1" }
            };

            var formValues = new FormCollection(formdic);

            var request = new Mock<HttpRequest>(MockBehavior.Strict);
            request.Setup(g => g.Method).Returns("POST");
            request.SetupGet(j => j.Form).Returns(formValues); 
            var actionContext = new ActionContext(mockHttpContext.Object, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor());
            productsController2.ControllerContext = new ControllerContext(actionContext); 

            var result = productsController2.Edit(1).Result;
            mock.Verify();
            if (result is RedirectToActionResult result1)
            {
                Assert.Equal("Index", result1.ActionName);
            }
            ///////////////////////////////////////////////////////Model invalid///////////////////////////////////////////////////////////
            List<ProductCategoryViewModel> ProductCategories = new();
            List<ProductSubCategoryViewModel> ProductSubCategories = new();
            formdic["ID"] = "";
            mock = new Mock<MockHttpMessageHandler>(MockBehavior.Strict);
            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsoluteUri.StartsWith("http://localhost:7223/api/ProductCategories")),
                ItExpr.IsAny<CancellationToken>()).ReturnsAsync(
            new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(ProductCategories), System.Text.Encoding.UTF8, "application/json")
            }).Verifiable();

            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsoluteUri.StartsWith("http://localhost:7223/api/ProductSubCategories/GetSubCategoriesByCategory/1")),
                ItExpr.IsAny<CancellationToken>()).ReturnsAsync(
            new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(ProductSubCategories), System.Text.Encoding.UTF8, "application/json")
            }).Verifiable();

            List<TaxGroupViewModel> taxGroupModels = new();
            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsoluteUri.StartsWith("http://localhost:7223/api/TaxGroups/GetTaxGroups")),
                ItExpr.IsAny<CancellationToken>()).ReturnsAsync(
            new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(taxGroupModels), System.Text.Encoding.UTF8, "application/json")
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
            mocklogger2 = new Mock<ILogger<EShopWeb.Areas.Admin.Controllers.ProductsController>>();
            productsController2 = new EShopWeb.Areas.Admin.Controllers.ProductsController(cryptoParams.Object, httpClientFactory.Object, configure.Object, contextAccessor.Object, mocklogger2.Object);

            request = new Mock<HttpRequest>(MockBehavior.Strict);
            request.Setup(g => g.Method).Returns("POST");
            request.SetupGet(j => j.Form).Returns(formValues); 
            actionContext = new ActionContext(mockHttpContext.Object, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor());
            productsController2.ControllerContext = new ControllerContext(actionContext); 

            result = productsController2.Edit(1).Result as ViewResult;
            mock.Verify();
            if (result is ViewResult result2)
            {
                Assert.Equal("Edit", result2.ViewName);
            } 
        }

        [Fact()]
        public void DeleteTest()
        {
            ProductViewModel Product = new(1, 1, 1, "", "", "", 0, 0, false, 0, Array.Empty<byte>(), Array.Empty<byte>(), cryptoParams.Object)
            {
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            InitController(Product);
            var nullidresult = productsController_Admin.Delete(null).Result;
            Assert.IsType<HttpStatusCodeWithBodyResult>(nullidresult);

            ViewResult viewResult = productsController_Admin.Delete(1).Result as ViewResult;
            mock.Verify();
            Assert.NotNull(viewResult.ViewData.Model);
            Assert.IsType<Product>(viewResult.ViewData.Model); 
        }
 

        [Fact()]
        public void GetProductDetailsTest()
        {
            List<InventoryViewModel> inventories = new();

            InitController(inventories);
            JsonResult result = productsController.GetProductDetails(1).Result as JsonResult;
            mock.Verify();
            Assert.NotNull(result);
            IDictionary<string, object> data = new RouteValueDictionary(result.Value);
            foreach (var item in data)
            {
                Assert.Equal("inventories", item.Key);
                Assert.IsType<List<InventoryViewModel>>(item.Value);
            } 
        }
    }
}