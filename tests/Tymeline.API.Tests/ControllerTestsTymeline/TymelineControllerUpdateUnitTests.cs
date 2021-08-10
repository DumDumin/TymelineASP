using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
    public class TymelineControllerUpdateUnitTest : OneTimeSetUpAttribute{
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
            _tymelineService.Setup(s => s.UpdateById(It.IsAny<string>(),It.IsAny<TymelineObject>())).Returns((string id,TymelineObject cc) => MockUpdateById(id,cc));
        }

        TymelineObject MockUpdateById(string id, TymelineObject tymelineObject){
            if(id == tymelineObject.Id){
                return tymelineObject;
            }
            else{
                throw new ArgumentException("the id and the passed in object do not match!");
            }
        }

        [Test]
        public async Task Test_TymelineUpdate_With_Existing_Entry_Returns_Existing_Entry_And_200() {
            TymelineObject tymelineObject = new TymelineObject("1",189890,new Content("testContent"),10000000,false,false);
            var obj = new IUpdateTymelineObject();
            obj.Id = "1";
            obj.tymelineObject = tymelineObject;
            var content = JsonContent.Create(obj);
           
            var response = await _client.PostAsync($"https://localhost:5001/tymeline/update",content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(HttpStatusCode.OK,statusCode);
        }

        [Test]
        public async Task Test_TymelineUpdate_With_New_Entry_Returns_New_Entry_And_201() {
            TymelineObject tymelineObject = new TymelineObject("1",189890,new Content("testContent"),10000000,false,false);
            var obj = new IUpdateTymelineObject();
            obj.Id = "1";
            obj.tymelineObject = tymelineObject;
            var content = JsonContent.Create(obj);
        
            var response = await _client.PostAsync($"https://localhost:5001/tymeline/update",content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(HttpStatusCode.OK,statusCode);
        }
    }
}