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
    public class TymelineControllerUpdateUnitTest : OneTimeSetUpAttribute
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private Moq.Mock<ITymelineService> _tymelineService;
        private Moq.Mock<IAuthService> _authService;

        public TestState state;

        Moq.Mock<IDataRolesService> _dataRolesService;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _factory = new WebApplicationFactory<Startup>();
            _tymelineService = new Moq.Mock<ITymelineService>();
            _authService = new Mock<IAuthService>();
            _dataRolesService = new Moq.Mock<IDataRolesService>();


            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped<ITymelineService>(s => _tymelineService.Object);
                    services.AddTransient<IAuthService>(s => _authService.Object);
                    services.AddScoped<IDataRolesService>(s => _dataRolesService.Object);
                });
            }).CreateClient();

            state = new TestState();
            // tymelineList = TestUtil.setupTymelineList();
            // roleList = TestUtil.CreateRoleList();
            // userdict = TestUtil.createUserDict();
            // userRoles = TestUtil.createRoleDict(userdict, roleList);

            // tymelineObjectRoles = TestUtil.setupRoles(tymelineList, roleList);
            _tymelineService.Setup(s => s.GetAllForUser(It.IsAny<string>(), It.IsAny<Roles>())).Returns((string userId, Roles minRole) => state.mockGetAll(userId, minRole));
            _tymelineService.Setup(s => s.UpdateById(It.IsAny<string>(), It.IsAny<TymelineObject>())).Returns((string id, TymelineObject cc) => state.MockUpdateById(id, cc));
            _authService.Setup(s => s.GetUserRoles(It.IsAny<string>())).Returns((string email) => TestUtil.mockGetUserPermissions(email));
            _authService.Setup(s => s.Login(It.IsAny<IUserCredentials>())).Returns((UserCredentials cc) => state.MockLogin(cc));
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
        public async Task Test_TymelineUpdate_With_Existing_Entry_Returns_Existing_Entry_And_200()
        {
            var userObjects = await getAll();
            var randomId = userObjects.RandomElement().Id;
            TymelineObject tymelineObject = new TymelineObject(randomId, 189890, new Content("testContent"), 10000000, false, false);
            var obj = new IUpdateTymelineObject();
            obj.Id = randomId;
            obj.tymelineObject = tymelineObject;
            var content = JsonContent.Create(obj);

            var response = await _client.PostAsync($"https://localhost:5001/tymeline/update", content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(HttpStatusCode.OK, statusCode);
        }

        [Test]
        public async Task Test_TymelineUpdate_With_New_Entry_Returns_New_Entry_And_403()
        {
            TymelineObject tymelineObject = new TymelineObject("999", 189890, new Content("testContent"), 10000000, false, false);
            var obj = new IUpdateTymelineObject();
            obj.Id = "999";
            obj.tymelineObject = tymelineObject;
            var content = JsonContent.Create(obj);

            var response = await _client.PostAsync($"https://localhost:5001/tymeline/update", content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(HttpStatusCode.Forbidden, statusCode);
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