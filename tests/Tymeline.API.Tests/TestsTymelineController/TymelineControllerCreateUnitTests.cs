using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Tymeline.API.Tests
{
    [TestFixture]
    [Category("HTTP")]
    public class TymelineControllerCreateUnitTest : OneTimeSetUpAttribute{

        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private Moq.Mock<ITymelineService> _tymelineService;
        private Moq.Mock<IAuthService> _authService;

        private AppSettings _configuration;


        public static AppSettings GetApplicationConfiguration()
        {
            var configuration = new AppSettings();

            var iConfig = new ConfigurationBuilder()
            
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            iConfig
                .GetSection("AppSettings")
                .Bind(configuration);

            return configuration;
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _factory = new WebApplicationFactory<Startup>();
            _tymelineService = new Moq.Mock<ITymelineService>();
            _authService = new Mock<IAuthService>();
            

            _configuration = GetApplicationConfiguration();

            _client = _factory.WithWebHostBuilder(builder =>
            {   
                builder.ConfigureTestServices(services => 
                {   
                    services.AddScoped<ITymelineService>(s => _tymelineService.Object);
                    services.AddScoped<IAuthService>(s => _authService.Object);
                    services.AddSingleton(_configuration);
                });
            }).CreateClient();
            _tymelineService.Setup(s => s.Create(It.IsAny<TymelineObject>())).Returns((TymelineObject tO) => mockCreateTymelineObject(tO));

        }

        private TymelineObject mockCreateTymelineObject(TymelineObject tO){
            tO.Id = new Guid().ToString();
            return tO;
        }


          [Test]
        public async Task Test_TymelineCreate_With_New_Entry_Returns_New_Entry_And_201() {
            TymelineObject tymelineObject = new TymelineObject("1",189890,new Content("testContent"),10000000,false,false);
           
            JsonContent content =  JsonContent.Create(tymelineObject);


            var response = await _client.PostAsync($"https://localhost:5001/tymeline/create",content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(HttpStatusCode.Created,statusCode);
        }

    }
}