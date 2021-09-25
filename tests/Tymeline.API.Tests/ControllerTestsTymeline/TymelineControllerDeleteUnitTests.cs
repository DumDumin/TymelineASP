using System;
using System.Collections.Generic;
using System.Linq;
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
    public class TymelineControllerDeleteUnitTest : OneTimeSetUpAttribute
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private Moq.Mock<ITymelineService> _tymelineService;
        private Moq.Mock<IAuthService> _authService;
        private List<TymelineObject> tymelineList;

        Dictionary<string, IUser> userdict;
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
                    services.AddTransient<IAuthService>(s => _authService.Object);
                });
            }).CreateClient();
            userdict = TestUtil.createUserDict();
            _tymelineService.Setup(s => s.Create(It.IsAny<TymelineObject>())).Returns((TymelineObject tO) => TestUtil.mockCreateTymelineObject(tO));
            _authService.Setup(s => s.GetUserRoles(It.IsAny<string>())).Returns((string email) => TestUtil.mockGetUserPermissions(email));
            _authService.Setup(s => s.Login(It.IsAny<IUserCredentials>())).Returns((UserCredentials cc) => MockLogin(cc));
            _tymelineService.Setup(s => s.DeleteById(It.IsAny<string>())).Callback((string id) => mockDeleteById(id));
        }
         private async Task<UserCredentials> Login()
        {
            UserCredentials credentials = new UserCredentials("test5@email.de", "hunter12");
            JsonContent content = JsonContent.Create(credentials);

            Uri uriLogin = new Uri("https://localhost:5001/auth/login");
            var response = await _client.PostAsync(uriLogin.AbsoluteUri, content);
            var result = response.Content.ReadAsStringAsync().Result;
            var user = JsonConvert.DeserializeObject<User>(result);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            IEnumerable<string> cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
            string jwt = cookies.First(s => s.StartsWith("jwt"));
            jwt = jwt.Split(";").First(s => s.StartsWith("jwt")).Replace("jwt=", "");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            _client.DefaultRequestHeaders.Add("Cookie",jwt);
            return credentials;
        }

        [SetUp]
        public async Task SetUpAsync()
        {   
            tymelineList = TestUtil.setupTymelineList();
            await Login();
        }


        [TearDown]
        public void TearDown()
        {
            _client.DefaultRequestHeaders.Clear();
         
        }


        [Test]
        public async Task Test_TymelineDelete_With_Existing_Entry_Returns_204()
        {

            await Login();
            JsonContent content = JsonContent.Create("10");
            var response = await _client.PostAsync($"https://localhost:5001/tymeline/delete", content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(HttpStatusCode.NoContent, statusCode);
        }



        [Test]
        public async Task Test_TymelineDelete_With_Existing_Entry_Returns_204_Expect_element_to_be_removed()
        {
            await Login();
            var key = "10";
            JsonContent content = JsonContent.Create(key);
            var r = await _client.PostAsync($"https://localhost:5001/tymeline/delete", content);
            var responseString = await r.Content.ReadAsStringAsync();
            var response = await _client.GetAsync($"https://localhost:5001/tymeline/get/{key}");
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            Assert.IsEmpty(responseString);
        }

        [Test]
        public async Task Test_TymelineDelete_With_NotExisting_Entry_204()
        {

            await Login();
            JsonContent content = JsonContent.Create("1000");

            var response = await _client.PostAsync($"https://localhost:5001/tymeline/delete", content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(HttpStatusCode.NoContent, statusCode);
        }


        private void mockDeleteById(string id)
        {
            var element = tymelineList.Find(element => element.Id.Equals(id));
            tymelineList.Remove(element);
        }


        public IUser MockLogin(UserCredentials credentials)
        {
            if (credentials.complete())
            {
                // check if user is registered
                if (userdict.ContainsKey(credentials.Email))
                {
                    return TestUtil.MockPasswdCheck(credentials.Password, userdict[credentials.Email]);
                }
                throw new ArgumentException();
            }
            else
            {
                throw new ArgumentException();
            }
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
                    throw new ArgumentException("key does not exist in the result");
                default:
                    throw new ArgumentException("there can only ever be one result with any given id");
            }
        }
    }
}