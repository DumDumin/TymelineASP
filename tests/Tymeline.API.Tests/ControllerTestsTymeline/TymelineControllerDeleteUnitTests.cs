using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
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
        private List<TymelineObject> tymelineList;


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
            _tymelineService.Setup(s => s.DeleteById(It.IsAny<string>())).Callback((string id) => mockDeleteById(id));
        }


        [SetUp]
        public void SetUp(){
            tymelineList = TestUtil.setupTymelineList();
        }

        private void mockDeleteById(string id){
            var element = tymelineList.Find(element => element.Id.Equals(id));
            tymelineList.Remove(element);
        }



        private TymelineObject mockTymelineReturnById(string identifier)
        
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
        public async Task Test_TymelineDelete_With_Existing_Entry_Returns_204() {


            JsonContent content =  JsonContent.Create("10");
            var response = await _client.PostAsync($"https://localhost:5001/tymeline/delete",content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(HttpStatusCode.NoContent,statusCode);
        }



        [Test]
        public async Task Test_TymelineDelete_With_Existing_Entry_Returns_204_Expect_element_to_be_removed() {

            var key = "10";
            JsonContent content =  JsonContent.Create(key);
            var r = await _client.PostAsync($"https://localhost:5001/tymeline/delete",content);
            var responseString = await r.Content.ReadAsStringAsync();
            var response = await _client.GetAsync($"https://localhost:5001/tymeline/get/{key}");
            Assert.AreEqual(HttpStatusCode.NoContent,response.StatusCode);
            Assert.IsEmpty(responseString);
        }   

          [Test]
        public async Task Test_TymelineDelete_With_NotExisting_Entry_204() {
            

            JsonContent content =  JsonContent.Create("1000");
            
            var response = await _client.PostAsync($"https://localhost:5001/tymeline/delete",content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(HttpStatusCode.NoContent,statusCode);
        }
    }
}