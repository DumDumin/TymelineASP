using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Tymeline.API.Controllers;

namespace Tymeline.API.Tests
{
    public class AuthControllerTests : OneTimeSetUpAttribute
    {
        Moq.Mock<ILogger<AuthController>> logger;
        Moq.Mock<IAuthService> authService;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            logger = new Moq.Mock<ILogger<AuthController>>();
            authService = new Moq.Mock<IAuthService>();
        }

        [Test]
        public async Task Test_Dependency()
        {
            var controller = new Tymeline.API.Controllers.AuthController(logger.Object, authService.Object);
        } 
    }
}