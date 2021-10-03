using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Tymeline.API.Tests
{
    [TestFixture]
    [Category("HTTP")]
    public class RolesWithRestictionsUnitTest : OneTimeSetUpAttribute
    {

        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private Moq.Mock<ITymelineService> _tymelineService;
        private Moq.Mock<IAuthService> _authService;
        private Moq.Mock<IDataRolesService> _dataRolesService;
        private AppSettings _configuration;

        public TestState state;

        public static AppSettings GetApplicationConfiguration()
        {
            var configuration = new AppSettings();

            var iConfig = new ConfigurationBuilder()

            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            iConfig
                .GetSection("AppSettings")
                .Bind(configuration);

            return configuration;
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _factory = new WebApplicationFactory<Startup>();
            _tymelineService = new Moq.Mock<ITymelineService>();
            _authService = new Moq.Mock<IAuthService>();
            _dataRolesService = new Moq.Mock<IDataRolesService>();

            _configuration = GetApplicationConfiguration();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped<ITymelineService>(s => _tymelineService.Object);
                    services.AddTransient<IAuthService>(s => _authService.Object);
                    services.AddScoped<IDataRolesService>(s => _dataRolesService.Object);
                    services.AddSingleton(_configuration);
                });
            }).CreateClient();


            state = new TestState();

            _tymelineService.Setup(s => s.GetAllForUser(It.IsAny<string>(), It.IsAny<Roles>())).Returns((string userId, Roles minRole) => state.mockGetAll(userId, minRole));
            _tymelineService.Setup(x => x.GetById(It.IsAny<string>())).Returns((string id) => state.mockGetById(id));
            _tymelineService.Setup(x => x.GetByTime(It.IsAny<int>(), It.IsAny<int>())).Returns((int start, int end) => state.mockDaoGetByTime(start, end));
            _tymelineService.Setup(x => x.DeleteById(It.IsAny<string>())).Callback((string id) => state.mockDeleteById(id));
            _tymelineService.Setup(x => x.Create(It.IsAny<TymelineObject>())).Returns((TymelineObject to) => state.mockCreate(to));
            _tymelineService.Setup(x => x.UpdateById(It.IsAny<string>(), It.IsAny<TymelineObject>())).Returns((string id, TymelineObject tO) => state.MockUpdateById(id, tO));
            _authService.Setup(s => s.GetUserRoles(It.IsAny<string>())).Returns((string email) => TestUtil.mockGetUserPermissions(email));
            _authService.Setup(s => s.Login(It.IsAny<IUserCredentials>())).Returns((UserCredentials cc) => state.MockLogin(cc));
            _dataRolesService.Setup(s => s.UserHasAccessToItem(It.IsAny<string>(), It.IsAny<string>())).Returns((string email, string itemId) => state.MockHasAccessToItem(email, itemId));
            _dataRolesService.Setup(s => s.GetItemRoles(It.IsAny<string>())).Returns((string itemId) => state.MockGetItemRoles(itemId));
            _dataRolesService.Setup(s => s.GetUserRoles(It.IsAny<string>())).Returns((string userEmail) => state.MockGetUserRoles(userEmail));
            _dataRolesService.Setup(s => s.UserHasAccessToItem(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Roles>())).Returns((string Email, string toId, Roles role) => state.MockHasAccessToItem(Email, toId, role));
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

        private async Task<TymelineObject> GetObject(string id)
        {
            var response = await _client.GetAsync($"https://localhost:5001/tymeline/get/{id}");
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TymelineObject>(responseString);
        }


        [Test]
        public async Task TestSetRoles_With_SupervisoryRole_Expect_Success()
        {
            await Login();
        }
        [Test]
        public async Task TestSetRoles_Without_SupervisoryRole_Expect_Forbidden()
        {
            await Login();
        }

        [Test]
        public async Task TestGetPermissionsForItem_With_SupervisoryRole_Expect_Success()
        {
            var user = await Login();
            var items = await getAll();
            var withPermission = state.userRoles[user.Email].Where(s => (int)Enum.Parse<Roles>(s.Value) > 1).ToList().RandomElement();
            var itemWithPermission = state.tymelineObjectRoles.Where(kw => kw.Value.Contains(withPermission)).ToList().RandomElement();
            var response = await _client.GetAsync($"https://localhost:5001/roles/getroles/item/{itemWithPermission.Key}");
            var responseString = await response.Content.ReadAsStringAsync();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        [Test]
        public async Task TestGetPermissionsForItem_Without_SupervisoryRole_Expect_Forbidden()
        {
            var user = await Login();
            var items = await getAll();
            var itemWithoutRights = state.tymelineList.RandomElementWithout(items);
            var response = await _client.GetAsync($"https://localhost:5001/roles/getroles/item/{itemWithoutRights.Id}");
            var responseString = await response.Content.ReadAsStringAsync();
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task TestAddPermissionForItem_With_SupervisoryRole_Expect_Success()
        {
            await Login();
        }
        [Test]
        public async Task TestAddPermissionForItem_Without_SupervisoryRole_Expect_Forbidden()
        {
            await Login();
        }

        [Test]
        public async Task TestRemovePermissionForItem_With_SupervisoryRole_Expect_Success()
        {
            await Login();
        }
        [Test]
        public async Task TestRemovePermissionForItem_Without_SupervisoryRole_Expect_Forbidden()
        {
            await Login();
        }




        [Test]
        public async Task TestAddPermission_With_SupervisoryRole_Expect_Success()
        {
            await Login();
        }
        [Test]
        public async Task TestAddPermission_Without_SupervisoryRole_Expect_Forbidden()
        {
            await Login();
        }
        [Test]
        public async Task TestRemovePermission_With_SupervisoryRole_Expect_Success()
        {
            await Login();
        }
        [Test]
        public async Task TestRemovePermission_Without_SupervisoryRole_Expect_Forbidden()
        {
            await Login();
        }


    }
}