using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using EShopModels;
using EShopWeb.Filters;
using EShopWeb.Common;

namespace EShopWeb.Controllers.Tests
{
    public class AccountControllerTests
    {
        private readonly Mock<IHttpContextAccessor> contextAccessor;
        private readonly AccountController accountController; 
        private readonly Mock<IHttpClientFactory> httpClientFactory;
        private readonly Mock<ILogger<AccountController>> mocklogger; 
        private readonly Mock<Microsoft.AspNetCore.Http.HttpContext> mockHttpContext;
        private readonly Mock<CryptoParamsProtector> cryptoParams;
         
        public AccountControllerTests()
        {
            cryptoParams = new Mock<CryptoParamsProtector>(MockBehavior.Strict);
            string value = "1";
            byte[] val = new byte[] { 1 };
            var client = new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:20731")
            };

            httpClientFactory = new Moq.Mock<IHttpClientFactory>();
            httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client); 
            contextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            Mock<ISession> mocks = new();
            mocks.Setup(c => c.Set("JWToken", It.IsAny<byte[]>())).Callback<string, byte[]>((k, v) => value = v.ToString());
            mocks.Setup(v => v.TryGetValue("JWToken", out val)).Returns(true);
            mockHttpContext.Setup(v => v.Session).Returns(mocks.Object);
            contextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object); 
            mocklogger = new Mock<ILogger<AccountController>>();
            accountController = new AccountController(httpClientFactory.Object,contextAccessor.Object,mocklogger.Object);
        }

        [Fact()]
        public void LogOutTest()
        {
            string value = "";
            LoginDetail loginDetail = new(1,DateTime.Now,DateTime.Now);
            loginDetail.ID = 1;
            Mock<MockHttpMessageHandler> mock = new(MockBehavior.Strict);
            mock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(res => res.RequestUri.AbsoluteUri.StartsWith("http://localhost:7223/api/EShopUsers/UpdateLoginDetails"))
            , ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
            {
                Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(loginDetail), System.Text.Encoding.UTF8, "application/json"),
                StatusCode = System.Net.HttpStatusCode.OK
            }).Verifiable();

            CookieOptions option = new();
            option.Expires = DateTime.Now.AddYears(-1);
            var response = new Mock<Microsoft.AspNetCore.Http.HttpResponse>(MockBehavior.Strict);
            response.Setup(j => j.Cookies.Append("logindetail", JsonConvert.SerializeObject(loginDetail), option));
            response.Setup(x => x.Cookies.Append("Cookie1", "", option));

            Mock<IResponseCookies> mockscookies = new();
            mockscookies.Setup(j => j.Append("logindetail", JsonConvert.SerializeObject(loginDetail), option));
            mockscookies.Setup(c => c.Append(Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.DefaultCookieName, Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.MakeCookieValue(new RequestCulture("en-GB")), It.IsAny<CookieOptions>()));
            response.SetupGet(v => v.Cookies).Returns(mockscookies.Object);

            value = "m";
            Mock<IRequestCookieCollection> mockrequestcookies = new();
            mockrequestcookies.Setup(f => f["logindetail"]).Returns(value);
            var request = new Mock<Microsoft.AspNetCore.Http.HttpRequest>(MockBehavior.Strict);
            request.Setup(a => a.Cookies).Returns(mockrequestcookies.Object);
            request.Setup(req => req.Method).Returns("POST");

            mockHttpContext.Setup(c => c.Request).Returns(request.Object);
            mockHttpContext.Setup(c => c.Response).Returns(response.Object);

            var actionContext = new ActionContext(mockHttpContext.Object, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor());
            accountController.ControllerContext = new ControllerContext(actionContext);

            RedirectToActionResult viewResult = accountController.LogOut() as RedirectToActionResult;
            Assert.Equal("Login", viewResult.ActionName);

            value = null;
            mockrequestcookies = new Mock<IRequestCookieCollection>();
            mockrequestcookies.Setup(f => f["logindetail"]).Returns(value);
            request = new Mock<Microsoft.AspNetCore.Http.HttpRequest>(MockBehavior.Strict);
            request.Setup(a => a.Cookies).Returns(mockrequestcookies.Object);
            request.Setup(req => req.Method).Returns("POST");

            mockHttpContext.Setup(c => c.Request).Returns(request.Object);
            mockHttpContext.Setup(c => c.Response).Returns(response.Object);

            actionContext = new ActionContext(mockHttpContext.Object, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor());
            accountController.ControllerContext = new ControllerContext(actionContext);

            viewResult = accountController.LogOut() as RedirectToActionResult;
            Assert.Equal("Login", viewResult.ActionName);
        }

    }
}