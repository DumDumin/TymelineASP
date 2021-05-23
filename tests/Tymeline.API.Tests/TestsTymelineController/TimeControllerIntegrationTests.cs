using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NUnit.Framework;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Tymeline.API.Tests
{

    public class TimeControllerIntegrationTests : OneTimeSetUpAttribute
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private Moq.Mock<ITimeService> _timeService;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _factory = new WebApplicationFactory<Startup>();
        }

        [SetUp]
        public void Setup()
        {
            _timeService = new Moq.Mock<ITimeService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services => 
                {
                    services.AddScoped<ITimeService>(s => _timeService.Object);
                });
            }).CreateClient();  
        }

        
        [TestCase("https://localhost:5001","/weatherforecast")]
        [TestCase("https://localhost:5001","/time")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url,string route)
        {
            // Act
            var response = await _client.GetAsync(url+route);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.AreEqual("application/json; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }

        [Test]
        public async Task Test_Dependency_Mock()
        {
            _timeService.Setup(s => s.GetTime()).Returns(10);

            var response = await _client.GetAsync("https://localhost:5001/time");

            Assert.AreEqual("10", await response.Content.ReadAsStringAsync());
        } 

        [Test]
        public async Task Test_Dependency()
        {
            _timeService.Setup(s => s.GetTime()).Returns(5);
            var response = await _client.GetAsync("https://localhost:5001/time");

            Assert.AreEqual("5", await response.Content.ReadAsStringAsync());
        } 



    }
}