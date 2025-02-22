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
using EShopModels.Common;

namespace EShopWeb.Controllers.Tests
{
    public class EShopUsersControllerTests
    {
        private Mock<IHttpContextAccessor> contextAccessor;
        private EShopUsersController EShopUsersController; 
        private Mock<IHttpClientFactory> httpClientFactory;
        private Mock<IOptions<EShopSystemConfig>> configure;
        private Mock<ILogger<EShopUsersController>> mocklogger;
        private Mock<Microsoft.AspNetCore.Http.HttpContext> mockHttpContext;
        private Mock<MockHttpMessageHandler> mock;
        private readonly Mock<CryptoParamsProtector> cryptoParams; 
 
        public EShopUsersControllerTests()
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
            contextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            Mock<ISession> mocks = new();
            mocks.Setup(c => c.Set("JWToken", It.IsAny<byte[]>())).Callback<string, byte[]>((k, v) => { var value = v.ToString(); });
            byte[] val = new byte[] { 1 };
            mocks.Setup(v => v.TryGetValue("JWToken", out val)).Returns(true);
            mockHttpContext.Setup(v => v.Session).Returns(mocks.Object);
            contextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);
            configure = new Mock<Microsoft.Extensions.Options.IOptions<EShopSystemConfig>>();
            mocklogger = new Mock<ILogger<EShopUsersController>>();
            EShopUsersController = new EShopUsersController(cryptoParams.Object, httpClientFactory.Object, configure.Object, contextAccessor.Object,mocklogger.Object);
        }
  
        [Fact()]
        public void IndexTest()
        {
            List<EShopUserViewModel> users = new();
            InitController(users);
            ViewResult result = EShopUsersController.Index(1).Result as ViewResult;
            mock.Verify();
            Assert.IsType<List<EShopUser>>(result.ViewData.Model); 
        }

     

        [Fact()]
        public void RegisterTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void RegisterTest1()
        {
            Assert.True(false, "This test needs an implementation");
        }
      
        [Fact()]
        public void LoginUserTest1()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void LogoffTest()
        {
            Assert.True(false, "This test needs an implementation");
        }
  
        [Fact()]
        public void DeleteTest()
        {
            EShopUserViewModel EShopUser = new(1, "", "", "", "", "", true, Guid.NewGuid(), Roles.User, Array.Empty<byte>(), cryptoParams.Object)
            {
                UserName = "Test",
                CreatedDate = DateTime.Now,
                FirstName = "",
                LastName = "",
                Address = "",
                Email = "",
                Password = "",
                ConfirmPassword = "",
                IsActive = true,
                ActivationCode = Guid.NewGuid() 
            };

            InitController(EShopUser);
            var nullidresult = EShopUsersController.Delete(null).Result;
            Assert.IsType<HttpStatusCodeWithBodyResult>(nullidresult);

            ViewResult viewResult = EShopUsersController.Delete(1).Result as ViewResult;
            mock.Verify();
            Assert.NotNull(viewResult.ViewData.Model);
            Assert.IsType<EShopUser>(viewResult.ViewData.Model); 
        }

        [Fact()]
        public void DeleteConfirmedTest()
        {
            EShopUserViewModel EShopUser = new(1, "", "", "", "", "", true, Guid.NewGuid(), Roles.User, Array.Empty<byte>(), cryptoParams.Object)
            {
                UserName = "Test",
                CreatedDate = DateTime.Now,
                FirstName = "",
                LastName = "",
                Address = "",
                Email = "",
                Password = "",
                ConfirmPassword = "",
                IsActive = true,
                ActivationCode = Guid.NewGuid() 
            };

            InitController(EShopUser);
            
            RedirectToActionResult routeResult = EShopUsersController.DeleteConfirmed(1).Result as RedirectToActionResult;
            mock.Verify();
            Assert.Equal("Index", routeResult.ActionName);
        }

        private void ValidateModel(string action, object model, EShopUsersController EShopUsersController)
        {
            var validationcontext = new System.ComponentModel.DataAnnotations.ValidationContext(model, null, null);

            RouteData route = new(); 
            route.Values.Add("action", action);
            route.Values.Add("Id", ((EShopUser)model).ID);
             
            var actionContext = new ActionContext(mockHttpContext.Object, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor());
            EShopUsersController.ControllerContext = new ControllerContext(actionContext);

            var valiationresults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(model, validationcontext, valiationresults, true);

            foreach (var valiationresult in valiationresults)
            {
                EShopUsersController.ModelState.AddModelError(valiationresult.MemberNames.First(), valiationresult.ErrorMessage);
            }
        }
    }
}