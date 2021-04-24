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

namespace Tymeline.API.Tests
{

    public class TymelineControllerIntegrationTest : OneTimeSetUpAttribute
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private Moq.Mock<ITymelineService> _tymelineService;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _factory = new WebApplicationFactory<Startup>();
        }

        [SetUp]
        public void Setup()
        {
            _tymelineService = new Moq.Mock<ITymelineService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services => 
                {
                    services.AddScoped<ITymelineService>(s => _tymelineService.Object);
                });
            }).CreateClient();  
            


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
                array.Add( new TymelineObject(i.ToString(),500+(random.Next() % 5000),new Content(RandomString(12)),10000+(random.Next() % 5000),RandomBool(),RandomBool()));
            }
            return array;
        }


        private TymelineObject mockTymelineReturnById(string identifier)
        
        {
            var array = TymelineControllerIntegrationTest.setupTymelineList();

            var results = from obj in array where obj.Id.Equals(identifier) select obj; 
            // TODO this needs refactoring ! @Mathias, dont use switches like this, there ought to be a better way
            // ask Tobias how to handle this, maybe just propagate the error, even if its not as desciptive.
            switch (results.Count())
            {
                case 1:
                    return results.ToList<TymelineObject>()[0];
                case 0:
                    throw new KeyNotFoundException("key does not exist in the result"); 
                default:
                    throw new ArgumentException("there can only ever be one result with any given id");
            }
           
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
            _tymelineService.Setup(s => s.getById(key)).Returns(mockTymelineReturnById(key));
            var response = await _client.GetAsync($"https://localhost:5001/tymeline/id/{key}");
            var responseString = await response.Content.ReadAsStringAsync();
            // ugly testing code . fix this!
            Assert.AreEqual(mockTymelineReturnById(key),JsonConvert.DeserializeObject<TymelineObject>(responseString));
        }

        [Test]
        public async Task Test_TymelineById_returns_204_forNotExistingElement()
        {
            

            string key = "105";

            // _tymelineService.Setup(s => s.getById(key)).Returns(mockTymelineReturnById(key));
            _tymelineService.Setup(s => s.getById(key)).Callback(() => mockTymelineReturnById(key));
            var response = await _client.GetAsync($"https://localhost:5001/tymeline/id/{key}");
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(204,(int)statusCode);
        }


        [Test]
        public async Task Test_TymelineById_returns_204_forNotExistingElement_WithMockedException()
        {
            

            string key = "99";

            // array.Add( new TymelineObject("99",500+(random.Next() % 5000),new Content(RandomString(12)),10000+(random.Next() % 5000),RandomBool(),RandomBool()));
            _tymelineService.Setup(s => s.getById(key)).Throws(new KeyNotFoundException());
            var response = await _client.GetAsync($"https://localhost:5001/tymeline/id/{key}");
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(204,(int)statusCode);
        }

          [Test]
        public async Task Test_TymelineById_returns_500_forBackendError()
        {
            string key = "99";

            // array.Add( new TymelineObject("99",500+(random.Next() % 5000),new Content(RandomString(12)),10000+(random.Next() % 5000),RandomBool(),RandomBool()));
            _tymelineService.Setup(s => s.getById(key)).Throws(new ArgumentException());
            var response = await _client.GetAsync($"https://localhost:5001/tymeline/id/{key}");
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(500,(int)statusCode);
        }
    }
}



