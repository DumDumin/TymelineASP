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
        private TestState state;
        private Moq.Mock<ITymelineService> _tymelineService;
        private Moq.Mock<IAuthService> _authService;
        private List<TymelineObject> tymelineList;
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
            _tymelineService.Setup(s => s.GetAllForUser(It.IsAny<string>(), It.IsAny<Roles>())).Returns((string email, Roles minRole) => state.mockGetAll(email, minRole));
            _tymelineService.Setup(s => s.Create(It.IsAny<TymelineObject>())).Returns((TymelineObject tO) => TestUtil.mockCreateTymelineObject(tO));
            _tymelineService.Setup(s => s.DeleteById(It.IsAny<string>())).Callback((string id) => state.mockDeleteById(id));
            _tymelineService.Setup(s => s.GetById(It.IsAny<string>())).Returns((string key) => state.MockTymelineReturnById(key));
            _authService.Setup(s => s.GetUserRoles(It.IsAny<string>())).Returns((string email) => TestUtil.mockGetUserPermissions(email));
            _authService.Setup(s => s.Login(It.IsAny<IUserCredentials>())).Returns((UserCredentials cc) => state.MockLogin(cc));
            _dataRolesService.Setup(s => s.UserHasAccessToItem(It.IsAny<string>(), It.IsAny<string>())).Returns((string email, string itemId) => state.MockHasAccessToItem(email, itemId));
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
        public async Task Test_TymelineDelete_With_Existing_Entry_Returns_OK()
        {

            await Login();
            var userObjects = await getAll();
            var randomId = userObjects.RandomElement().Id;
            JsonContent content = JsonContent.Create(randomId);
            var response = await _client.PostAsync($"https://localhost:5001/tymeline/delete", content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(HttpStatusCode.OK, statusCode);
        }



        [Test]
        public async Task Test_TymelineDelete_With_Existing_Entry_Returns_OK_Expect_element_to_be_removed()
        {
            await Login();
            var userObjects = await getAll();
            var randomId = userObjects.RandomElement().Id;
            JsonContent content = JsonContent.Create(randomId);
            var r = await _client.PostAsync($"https://localhost:5001/tymeline/delete", content);
            var responseString = await r.Content.ReadAsStringAsync();
            var response = await _client.GetAsync($"https://localhost:5001/tymeline/get/{randomId}");
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.IsEmpty(responseString);
        }

        [Test]
        public async Task Test_TymelineDelete_With_NotExisting_Entry_403()
        {

            await Login();
            var userObjects = await getAll();
            var randomId = state.tymelineList.RandomElementWithout(userObjects).Id;
            JsonContent content = JsonContent.Create(randomId);

            var response = await _client.PostAsync($"https://localhost:5001/tymeline/delete", content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
        }

        private async Task<List<TymelineObject>> getAll()
        {

            var response = await _client.GetAsync("https://localhost:5001/tymeline/get");
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<TymelineObject>>(responseString);
        }




    }
}