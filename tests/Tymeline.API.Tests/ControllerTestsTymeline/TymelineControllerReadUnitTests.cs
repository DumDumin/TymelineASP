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
using System.Net.Http.Json;
using System.Net.Http.Headers;
using FluentAssertions;

namespace Tymeline.API.Tests
{
    [TestFixture]
    [Category("HTTP")]
    public class TymelineControllerReadUnitTest : OneTimeSetUpAttribute
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private TestState state;
        private Moq.Mock<ITymelineService> _tymelineService;
        private Moq.Mock<IAuthService> _authService;
        private Mock<IDataRolesService> _dataRolesService;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _factory = new WebApplicationFactory<Startup>();
            _tymelineService = new Moq.Mock<ITymelineService>();
            _authService = new Mock<IAuthService>();
            _dataRolesService = new Mock<IDataRolesService>();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped<ITymelineService>(s => _tymelineService.Object);
                    services.AddTransient<IAuthService>(s => _authService.Object);
                    services.AddTransient<IDataRolesService>(s => _dataRolesService.Object);
                });
            }).CreateClient();
            state = new TestState();
            _tymelineService.Setup(s => s.GetById(It.IsAny<string>())).Returns((string key) => state.MockTymelineReturnById(key));
            _tymelineService.Setup(s => s.GetByTime(It.IsAny<int>(), It.IsAny<int>())).Returns((int start, int end) => state.MockTymelineReturnByTime(start, end));
            _authService.Setup(s => s.GetUserRoles(It.IsAny<string>())).Returns((string email) => TestUtil.mockGetUserPermissions(email));
            _authService.Setup(s => s.Login(It.IsAny<IUserCredentials>())).Returns((UserCredentials cc) => state.MockLogin(cc));
            // _tymelineService.Setup(s => s.GetAll()).Returns(state.tymelineList);
            _tymelineService.Setup(s => s.GetAllForUser(It.IsAny<string>(), It.IsAny<Roles>())).Returns((string email, Roles minRole) => state.mockGetAll(email, minRole));
            _dataRolesService.Setup(s => s.UserHasAccessToItem(It.IsAny<string>(), It.IsAny<string>())).Returns((string email, string itemId) => state.MockHasAccessToItem(email, itemId));
        }


        [SetUp]
        public async Task SetUpAsync()
        {
            await Login();
        }

        [TearDown]
        public void TearDown()
        {
            _client.DefaultRequestHeaders.Clear();

        }


        [Test]
        public async Task Test_TymelinegetAll_returnsValidJSON_forListTymelineObject()
        {
            var response = await _client.GetAsync("https://localhost:5001/tymeline/get");
            var responseString = await response.Content.ReadAsStringAsync();
            var tOList = JsonConvert.DeserializeObject<List<TymelineObject>>(responseString);
            state.tymelineList.Should().Contain(tOList[0]);
            tOList.Select(to => state.tymelineList.Should().Contain(to));
            // JsonConvert.DeserializeObject<List<TymelineObject>>(responseString).Should().BeSubsetOf(state.tymelineList);
            JsonConvert.DeserializeObject<List<TymelineObject>>(responseString).Should().HaveCountLessOrEqualTo(state.tymelineList.Count);
        }


        [Test]
        public async Task Test_TymelinegetById_returnsValidJSON_forTymelineObject()
        {
            var userObjects = await getAll();
            var randomId = userObjects.RandomElement().Id;

            var response = await _client.GetAsync($"https://localhost:5001/tymeline/get/{randomId}");
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            state.MockTymelineReturnById(randomId).Should().BeEquivalentTo(JsonConvert.DeserializeObject<TymelineObject>(responseString));
        }

        [Test]
        public async Task Test_TymelineById_returns_403_forNotExistingElement()
        {

            string key = "105";
            var response = await _client.GetAsync($"https://localhost:5001/tymeline/get/{key}");
            var responseString = await response.Content.ReadAsStringAsync();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }


        [Test]
        public async Task Test_TymelineById_returns_403_forNotExistingElement_WithMockedException()
        {
            string key = "199";

            _tymelineService.Setup(s => s.GetById(key)).Throws(new ArgumentException());
            var response = await _client.GetAsync($"https://localhost:5001/tymeline/get/{key}");
            var responseString = await response.Content.ReadAsStringAsync();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        }




        [Test]
        public async Task Test_TymelineByTime_returns_200_forValidTime()
        {
            var start = 12000;
            var end = 13000;

            var response = await _client.GetAsync($"https://localhost:5001/tymeline/getbytime/{start}-{end}");
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(HttpStatusCode.OK, statusCode);

        }

        [Test]
        public async Task Test_TymelineByTime_ListOfTymelineObject_forValidTime()
        {
            var start = 12000;
            var end = 13000;

            var response = await _client.GetAsync($"https://localhost:5001/tymeline/getbytime/{start}-{end}");
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            state.MockTymelineReturnByTime(start, end)
            .Should()
            .BeEquivalentTo(JsonConvert.DeserializeObject<List<TymelineObject>>(responseString));
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
            _client.DefaultRequestHeaders.Add("Cookie", jwt);
            return credentials;
        }

        private async Task<List<TymelineObject>> getAll()
        {

            var response = await _client.GetAsync("https://localhost:5001/tymeline/get");
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<TymelineObject>>(responseString);
        }

    }
}



