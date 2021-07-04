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
using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Security.Principal;
using Microsoft.Extensions.Options;

namespace Tymeline.API.Tests
{
    [TestFixture]
    [Category("HTTP")]
    public class TymelineControllerReadUnitTest : OneTimeSetUpAttribute
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private Moq.Mock<ITymelineService> _tymelineService;
        private Moq.Mock<IAuthService> _authService;

        private List<TymelineObject> tymelineList;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _factory = new WebApplicationFactory<Startup>();
            _tymelineService = new Moq.Mock<ITymelineService>();
            _authService = new Mock<IAuthService>();
            tymelineList = TymelineControllerReadUnitTest.setupTymelineList();
            _client = _factory.WithWebHostBuilder(builder =>
            {   
                builder.ConfigureTestServices(services => 
                {   
                    services.AddScoped<ITymelineService>(s => _tymelineService.Object);
                    services.AddScoped<IAuthService>(s => _authService.Object);
                });
            }).CreateClient();
            _tymelineService.Setup(s => s.GetById(It.IsAny<int>())).Returns((int key) => mockTymelineReturnById(key));
            _tymelineService.Setup(s => s.GetByTime(It.IsAny<int>(),It.IsAny<int>())).Returns((int start, int end) => mockTymelineReturnByTime(start,end));
        }

        [SetUp]
        public void Setup()
        {
           
            


        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static bool RandomBool()
        {
            return (random.Next() % 2)==1;
        }

       


        static private List<TymelineObject> setupTymelineList(){
            // this will return different objects each run! be prepared to not test for anything but existance of some attributes
            // DO NOT TEST THE VALUE OF ATTRIBUTES NOT CREATED AS A MOCK SPECIFICALLY FOR USE IN THAT TEST
            // IT WILL BREAK AND YOU WILL HATE LIFE

            List<TymelineObject> array = new List<TymelineObject>();
            for (int i = 1; i < 100; i++)
            {

                array.Add( new TymelineObject() {
                    Id=i,
                    Length=500+(random.Next() % 5000),
                    Content=new Content(RandomString(12)),
                    Start=10000+(random.Next() % 5000),
                    CanChangeLength=RandomBool(),
                    CanMove=RandomBool()
                    }
                );
            }
            return array;
        }


        private List<TymelineObject> mockTymelineReturnByTime(int start,int end){
           
            var s = tymelineList.Where(element => start<element.Start+element.Length && start>element.Start+element.Length).ToList();
            s.AddRange(tymelineList.Where(element => start<element.Start && element.Start<end).ToList());
            return s.Distinct().ToList();
        }

        private TymelineObject mockTymelineReturnById(int identifier)
        
        {
            
            var results = tymelineList.Where(element => element.Id.Equals(identifier)).ToList();
            // var results = from obj in array where obj.Id.Equals(identifier) select obj; 

            switch (results.Count())
            {
                case 1:
                    return results[0];
                case 0:
                    throw new KeyNotFoundException("key does not exist in the result"); 
                default:
                    throw new ArgumentException("there can only ever be one result with any given id");
            }
        }
    



        [Test]
        public async Task Test_TymelinegetAll_returnsValidJSON_forListTymelineObject()
        {   
            _tymelineService.Setup(s => s.GetAll()).Returns(tymelineList); 
            var response = await _client.GetAsync("https://localhost:5001/tymeline/get");
            var responseString = await response.Content.ReadAsStringAsync();
            
            Assert.AreEqual(tymelineList,JsonConvert.DeserializeObject<List<TymelineObject>>(responseString));
        } 


          [Test]
        public async Task Test_TymelinegetById_returnsValidJSON_forTymelineObject()
        {   
        
            int key = 2;
            var response = await _client.GetAsync($"https://localhost:5001/tymeline/get/{key}");
            var responseString = await response.Content.ReadAsStringAsync();
            // ugly testing code . fix this!
            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode);
            Assert.AreEqual(mockTymelineReturnById(key),
            JsonConvert.DeserializeObject<TymelineObject>(responseString));
        }

        [Test]
        public async Task Test_TymelineById_returns_204_forNotExistingElement()
        {
        
            int key = 105;
            var response = await _client.GetAsync($"https://localhost:5001/tymeline/get/{key}");
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(HttpStatusCode.NoContent,statusCode);
        }


        [Test]
        public async Task Test_TymelineById_returns_204_forNotExistingElement_WithMockedException()
        {
            int key = 99;

            _tymelineService.Setup(s => s.GetById(key)).Throws(new KeyNotFoundException());
            var response = await _client.GetAsync($"https://localhost:5001/tymeline/get/{key}");
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(HttpStatusCode.NoContent,statusCode);
        }


        [Test]
        public async Task Test_TymelineById_returns_500_forBackendError()
        {
            
            int key = 99;
            
            _tymelineService.Setup(s => s.GetById(key)).Throws(new ArgumentException());
            var response = await _client.GetAsync($"https://localhost:5001/tymeline/get/{key}");
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(HttpStatusCode.InternalServerError,statusCode);
        }


        [Test]
        public async Task Test_TymelineByTime_returns_200_forValidTime()
        {
            var start = 12000;
            var end = 13000;
            
            var response = await _client.GetAsync($"https://localhost:5001/tymeline/getbytime/{start}-{end}");
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(HttpStatusCode.OK,statusCode);
       
        }

                [Test]
        public async Task Test_TymelineByTime_ListOfTymelineObject_forValidTime()
        {
            var start = 12000;
            var end = 13000;
            
            var response = await _client.GetAsync($"https://localhost:5001/tymeline/getbytime/{start}-{end}");
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(mockTymelineReturnByTime(start,end),JsonConvert.DeserializeObject<List<TymelineObject>>(responseString)
            );
        }

       

        
    }
}



