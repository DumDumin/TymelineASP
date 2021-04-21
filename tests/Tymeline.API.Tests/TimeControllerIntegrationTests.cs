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
        private Moq.Mock<ITymelineService> _tymelineService;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _factory = new WebApplicationFactory<Startup>();
        }

        [SetUp]
        public void Setup()
        {
            _timeService = new Moq.Mock<ITimeService>();
            _tymelineService = new Moq.Mock<ITymelineService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services => 
                {

                    services.AddScoped<ITymelineService>(s => _tymelineService.Object);
                    services.AddScoped<ITimeService>(s => _timeService.Object);
                });
            }).CreateClient();  
            


        }


        private List<TymelineObject> setupTymelineList(){

            List<TymelineObject> array = new List<TymelineObject>();
            var responseItem1 = new TymelineObject("1",10000,new Content("no content"),10000,true,false);
            var responseItem2 = new TymelineObject("2",10002,new Content("no contents"),10300,false,false);
            var responseItem3 = new TymelineObject("3",10001,new Content("no contenta"),12000,true,true);
            array.Add(responseItem1);
            array.Add(responseItem2);
            array.Add(responseItem3);
            return array;

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


        [Test]
        public async Task Test_TymelinegetAll_returnsValidJSON_forListTymelineObject()
        {   
            var array = setupTymelineList();
            _tymelineService.Setup(s => s.getAll()).Returns(array); 
            var response = await _client.GetAsync("https://localhost:5001/tymeline/all");
            var responseString = await response.Content.ReadAsStringAsync();
            
            Assert.AreEqual(array,JsonConvert.DeserializeObject<List<TymelineObject>>(responseString));
        } 


          [Test]
        public async Task Test_TymelinegetById_returnsValidJSON_forTymelineObject()
        {   
            var array = setupTymelineList();

            string key = "2";
            _tymelineService.Setup(s => s.getById(key)).Returns(array[1]);
            var response = await _client.GetAsync($"https://localhost:5001/tymeline/id/{key}");
            var responseString = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(array[1],JsonConvert.DeserializeObject<TymelineObject>(responseString));
        } 

    }
}