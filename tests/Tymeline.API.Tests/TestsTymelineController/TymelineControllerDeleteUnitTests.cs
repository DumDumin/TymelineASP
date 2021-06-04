using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Tymeline.API.Tests
{
    [TestFixture]
    [Category("HTTP")]
    public class TymelineControllerDeleteUnitTest : OneTimeSetUpAttribute{
         private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private Moq.Mock<ITymelineService> _tymelineService;
        private Moq.Mock<IAuthService> _authService;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _factory = new WebApplicationFactory<Startup>();
            _tymelineService = new Moq.Mock<ITymelineService>();
            _authService = new Mock<IAuthService>();
            
            _client = _factory.WithWebHostBuilder(builder =>
            {   
                builder.ConfigureTestServices(services => 
                {   
                    services.AddScoped<ITymelineService>(s => _tymelineService.Object);
                    services.AddScoped<IAuthService>(s => _authService.Object);
                });
            }).CreateClient();    
            _tymelineService.Setup(s => s.DeleteById(It.IsAny<string>())).Callback((string id) => {}).Verifiable();
        }

        [Test]
        public async Task Test_TymelineDelete_With_Existing_Entry_Returns_204() {


            JsonContent content =  JsonContent.Create("10");
            var response = await _client.PostAsync($"https://localhost:5001/tymeline/delete",content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(HttpStatusCode.NoContent,statusCode);
            _tymelineService.Verify();
        }

          [Test]
        public async Task Test_TymelineDelete_With_NotExisting_Entry_204() {
            

            JsonContent content =  JsonContent.Create("10");
            
            var response = await _client.PostAsync($"https://localhost:5001/tymeline/delete",content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(HttpStatusCode.NoContent,statusCode);
            _tymelineService.Verify();
        }
    }
}