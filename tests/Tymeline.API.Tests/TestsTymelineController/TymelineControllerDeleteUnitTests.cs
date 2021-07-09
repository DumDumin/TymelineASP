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
            tymelineList = TymelineControllerDeleteUnitTest.setupTymelineList();
        }

        private void mockDeleteById(string id){
            var element = tymelineList.Find(element => element.Id.Equals(id));
            tymelineList.Remove(element);
        }

          static private List<TymelineObject> setupTymelineList(){
            // this will return different objects each run! be prepared to not test for anything but existance of some attributes
            // DO NOT TEST THE VALUE OF ATTRIBUTES NOT CREATED AS A MOCK SPECIFICALLY FOR USE IN THAT TEST
            // IT WILL BREAK AND YOU WILL HATE LIFE

            List<TymelineObject> array = new List<TymelineObject>();
            for (int i = 1; i < 100; i++)
            {

                array.Add( new TymelineObject() {
                    Id=i.ToString(),
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