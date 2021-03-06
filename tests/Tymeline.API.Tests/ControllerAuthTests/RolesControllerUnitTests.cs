using Moq;
using System;
using NUnit.Framework;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

using System.Linq;

using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net;
using System.Security.Claims;
using AutoFixture.NUnit3;
using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using ExtensionMethods;

namespace Tymeline.API.Tests
{
    [TestFixture]
    [Category("HTTP")]
    public class RolesControllerUnitTests : OneTimeSetUpAttribute
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private Moq.Mock<IDataRolesService> _rolesService;
        private Moq.Mock<IAuthService> _authService;
        private JwtService _jwtService;
        private UtilService _utilService;
        private IOptions<AppSettings> _appSettingsOptions;
        AppSettings _appSettings;
        private Mock<IAuthDao> _authDao;
        private Moq.Mock<ITymelineService> _tymelineService;
        public TestState state;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {




            _appSettingsOptions = Options.Create<AppSettings>(new AppSettings());
            _appSettings = _appSettingsOptions.Value;
            _factory = new WebApplicationFactory<Startup>();
            _authService = new Moq.Mock<IAuthService>();
            _utilService = new UtilService();
            _rolesService = new Mock<IDataRolesService>();
            _tymelineService = new Mock<ITymelineService>();
            _jwtService = new JwtService(_authService.Object, _appSettingsOptions);

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped<ITymelineService>(s => _tymelineService.Object);
                    services.AddSingleton<IJwtService>(s => _jwtService);
                    services.AddSingleton<IAuthService>(s => _authService.Object);
                    services.AddSingleton<UtilService>(s => _utilService);
                    services.AddSingleton<IDataRolesService>(s => _rolesService.Object);
                });
            }).CreateClient();
            state = new TestState();

            _authService.Setup(s => s.Login(It.IsAny<IUserCredentials>())).Returns((UserCredentials cc) => state.MockLogin(cc));
            _tymelineService.Setup(s => s.GetAllForUser(It.IsAny<string>(), It.IsAny<Roles>())).Returns((string userId, Roles minRole) => state.mockGetAll(userId, minRole));
            _rolesService.Setup(s => s.GetUserRoles(It.IsAny<string>())).Returns((string email) => state.MockGetUserRoles(email));
            _rolesService.Setup(s => s.GetItemRoles(It.IsAny<string>())).Returns((string itemId) => state.MockGetItemRoles(itemId));
            _rolesService.Setup(s => s.SetUserRoles(It.IsAny<IUserRoles>())).Callback((IUserRoles permissions) => state.MockSetRoles(permissions));
            _rolesService.Setup(s => s.AddUserRole(It.IsAny<IRole>(), It.IsAny<string>())).Returns((string email, IRole permission) => state.MockAddRoleToUser(email, permission));
            _rolesService.Setup(s => s.AddUserRole(It.IsAny<IUserRole>())).Returns((IUserRole userRole) => state.MockAddRoleToUser(userRole.Email, userRole.Role));
            _rolesService.Setup(s => s.RemoveUserRole(It.IsAny<IRole>(), It.IsAny<string>())).Returns((string Email, IRole role) => state.MockRemoveUserRole(Email, role));
            _rolesService.Setup(s => s.RemoveUserRole(It.IsAny<IUserRole>())).Returns((IUserRole userRole) => state.MockRemoveUserRole(userRole.Email, userRole.Role));
            _rolesService.Setup(s => s.AddRoleToItem(It.IsAny<IRole>(), It.IsAny<string>())).Returns((IRole role, string to) => state.MockAddRoleToItem(role, to));
            _rolesService.Setup(s => s.RemoveRoleFromItem(It.IsAny<IRole>(), It.IsAny<string>())).Returns((IRole role, string to) => state.MockRemoveRoleFromItem(role, to));
            _rolesService.Setup(s => s.AddRole(It.IsAny<IRole>())).Callback((IRole role) => state.MockAddRole(role));
            _rolesService.Setup(s => s.RemoveRole(It.IsAny<IRole>())).Callback((IRole role) => state.MockRemoveRole(role));
            _rolesService.Setup(s => s.GetRoles()).Returns(() => state.MockGetRoles());
            _rolesService.Setup(s => s.UserHasAccessToItem(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Roles>())).Returns((string Email, string itemId, Roles minRole) => state.MockHasAccessToItem(Email, itemId, minRole));
            _authService.Setup(s => s.getUsers()).Returns(() => state.MockGetUsers());
            _authService.Setup(s => s.GetUserRoles(It.IsAny<string>())).Returns((string email) => state.MockGetUserPermissions(email));
        }


        [SetUp]
        public async Task SetupAsync()
        {
            await Login();
        }


        [TearDown]
        public void TearDown()
        {
            _client.DefaultRequestHeaders.Clear();

        }





        private async Task<UserCredentials> Login(string id = "5")
        {
            UserCredentials credentials = new UserCredentials($"test{id}@email.de", "hunter12");
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


        private async Task<User> GetRandomUser()
        {
            var response = await _client.GetAsync("https://localhost:5001/auth/users");
            var responseObject = await response.Content.ReadFromJsonAsync<List<User>>();
            var statusCode = response.StatusCode;
            User user = responseObject.RandomElement();
            return user;
        }

        private async Task<List<TymelineObject>> getAll()
        {
            var response = await _client.GetAsync("https://localhost:5001/tymeline/get");
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<TymelineObject>>(responseString);
        }


        private async Task<List<TymelineObject>> getWithRights()
        {
            var response = await _client.GetAsync($"https://localhost:5001/tymeline/getsuper/");
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<TymelineObject>>(responseString);
        }

        private JwtSecurityToken ExtractTokenFromCookies(IEnumerable<string> c)
        {
            string webtoken = c.First(s => s.StartsWith("jwt"));
            webtoken = webtoken.Split(";").First(s => s.StartsWith("jwt")).Replace("jwt=", "");
            var parsedJwt = _jwtService.verifyJwt(webtoken);
            return parsedJwt;
        }

        [Test, AutoData]
        public async Task Test_UserInfo_with_registeredAccount_Set_Permissions_Return_New_Permissions_inUserInfo(HttpUserRoles userPermissions)
        {
            var loginData = await Login();
            userPermissions.Email = loginData.Email;
            JsonContent jsonList = JsonContent.Create(userPermissions);

            Uri uriSetPermissions = new Uri("https://localhost:5001/roles/setroles");
            var setPermissions = await _client.PostAsync(uriSetPermissions, jsonList);
            var responseObjectsetPermissions = await setPermissions.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, setPermissions.StatusCode);

            Uri uriTest = new Uri("https://localhost:5001/roles/userInfo");
            var responseTest = await _client.GetAsync(uriTest.AbsoluteUri);
            var responseObject = await responseTest.Content.ReadAsStringAsync();
            var parsedObject = JsonConvert.DeserializeObject<UserRoles>(responseObject);
            parsedObject.Roles.Should().Contain(userPermissions.Roles[0]).And.Contain(userPermissions.Roles[1]).And.Contain(userPermissions.Roles[2]);
        }

        [Test, AutoData]
        public async Task Test_UserInfo_with_registeredAccount_Set_Role_Return_New_Role_inJWT(List<Role> testList)
        {

            //setup
            testList[0].Type = "Frontend";
            UserCredentials creds = await Login();

            //given
            HttpUserRoles expectedPermission = new HttpUserRoles(creds.Email, testList);
            Uri uriSetPermissions = new Uri("https://localhost:5001/roles/setroles");
            var setPermissions = await _client.PostAsync(uriSetPermissions, JsonContent.Create(expectedPermission));
            var responseObjectsetPermissions = await setPermissions.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, setPermissions.StatusCode);
            IEnumerable<string> c = setPermissions.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
            JwtSecurityToken parsedJwt = ExtractTokenFromCookies(c);
            parsedJwt.Claims.ToList().Select(claim => new Role(claim.Type, claim.Value)).ToList().Should().Contain(testList[0]);
        }



        [Test, AutoData]
        public async Task Test_UserInfo_with_registeredAccount_Set_Roles_Return_New_Role_inJWT(List<Role> testList)
        {

            //setup
            testList.ForEach(role => role.Type = "Frontend");
            UserCredentials creds = await Login();

            //given
            HttpUserRoles expectedPermission = new HttpUserRoles(creds.Email, testList);
            Uri uriSetPermissions = new Uri("https://localhost:5001/roles/setroles");
            var setPermissions = await _client.PostAsync(uriSetPermissions, JsonContent.Create(expectedPermission));
            var responseObjectsetPermissions = await setPermissions.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, setPermissions.StatusCode);
            IEnumerable<string> c = setPermissions.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
            JwtSecurityToken parsedJwt = ExtractTokenFromCookies(c);
            parsedJwt.Claims.ToList().Select(claim => new Role(claim.Type, claim.Value)).ToList().Should().Contain(testList);
        }

        [Test, AutoData]
        public async Task Test_Add_Role_With_Valid_Role(Role ep)
        {
            //setup
            UserCredentials creds = await Login();
            ep.Type = "Frontend";

            //given
            HttpUserRole expectedPermission = new HttpUserRole(creds.Email, ep);
            Uri uriAddPermission = new Uri("https://localhost:5001/roles/addroletouser");
            var setPermissions = await _client.PostAsync(uriAddPermission, JsonContent.Create(expectedPermission));
            var responseObject = await setPermissions.Content.ReadAsStringAsync();
            IEnumerable<string> cookies = setPermissions.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
            JwtSecurityToken parsedJwt = ExtractTokenFromCookies(cookies);
            //assert
            parsedJwt.Claims.ToList().Select(claim => new Role(claim.Type, claim.Value)).ToList().Should().Contain(expectedPermission.Role.As<Role>());
        }





        [Test, AutoData]

        public async Task Test_Get_Roles(Role ep)
        {
            UserCredentials creds = await Login();

            User user = await GetRandomUser();

            HttpUserRole expectedPermission = new HttpUserRole(user.Email, ep);
            Uri uriAddPermission = new Uri("https://localhost:5001/roles/addroletouser");
            var setPermissions = await _client.PostAsync(uriAddPermission, JsonContent.Create(expectedPermission));


            var roleResponse = await _client.GetAsync($"https://localhost:5001/roles/getroles/email/{user.Email}");

            var s = await roleResponse.Content.ReadAsStringAsync();
            var roleResponseObject = JsonConvert.DeserializeObject<HttpUserRoles>(s);
            var roleStatusCode = roleResponse.StatusCode;

            roleResponseObject.Email.Should().Be(user.Email);
            roleResponseObject.Roles.Should().BeOfType<List<Role>>();
            roleResponseObject.Roles.Should().Contain(ep);

        }


        [Test, AutoData]
        public async Task Test_Get_Roles_Set(List<Role> testList)
        {
            Uri uriSetPermissions = new Uri("https://localhost:5001/roles/setroles");
            //setup
            UserCredentials creds = await Login();

            User user = await GetRandomUser();

            //given

            HttpUserRoles expectedPermission = new HttpUserRoles(user.Email, testList); //create new HttpUserPermission
            var setPermissions = await _client.PostAsync(uriSetPermissions, JsonContent.Create(expectedPermission));
            var roleResponse = await _client.GetAsync($"https://localhost:5001/roles/getroles/email/{user.Email}");

            var s = await roleResponse.Content.ReadAsStringAsync();
            var roleResponseObject = JsonConvert.DeserializeObject<HttpUserRoles>(s);
            var roleStatusCode = roleResponse.StatusCode;

            roleResponseObject.Email.Should().Be(user.Email);
            roleResponseObject.Roles.Should().BeOfType<List<Role>>().And.Contain(testList);

        }

        [Test, AutoData]
        public async Task Test_Remove_Role_With_Valid_Role(Role ep, Role ep_keep)
        {
            // setup
            UserCredentials creds = await Login();
            User user = await GetRandomUser();
            HttpUserRole expectedPermission = new HttpUserRole(user.Email, ep);
            HttpUserRole expectedPermission_keep = new HttpUserRole(user.Email, ep_keep);
            Uri uriAddPermission = new Uri("https://localhost:5001/roles/addroletouser");

            //given

            await _client.PostAsync(uriAddPermission, JsonContent.Create(expectedPermission));
            await _client.PostAsync(uriAddPermission, JsonContent.Create(expectedPermission_keep));
            Uri uriRemovePermission = new Uri("https://localhost:5001/roles/removerolefromuser");
            await _client.PostAsync(uriRemovePermission, JsonContent.Create(expectedPermission));

            //assert

            var roleResponse = await _client.GetAsync($"https://localhost:5001/roles/getroles/email/{user.Email}");

            var s = await roleResponse.Content.ReadAsStringAsync();
            var roleResponseObject = JsonConvert.DeserializeObject<HttpUserRoles>(s);
            var roleStatusCode = roleResponse.StatusCode;

            roleResponseObject.Email.Should().Be(user.Email);
            roleResponseObject.Roles.Should().BeOfType<List<Role>>().And.Contain(ep_keep).And.NotContain(ep);


        }

        [Test, AutoData]
        public async Task Test_AddRoleToItem_given_NewRole_expect_NewRoleCreation(Role role)
        {  // setup
            UserCredentials creds = await Login();
            TymelineObject tymelineObject = await getAll().ContinueWith(s => s.Result.RandomElement());
            var payload = new HttpTymelineObjectRolesIncrement { Role = role, tymelineObjectId = tymelineObject.Id };

            var roleResponse = await _client.PostAsync($"https://localhost:5001/roles/additemrole", JsonContent.Create(payload));

            var response = await roleResponse.Content.ReadAsStringAsync();
            HttpTymelineObjectRoles roleResponseObject = JsonConvert.DeserializeObject<HttpTymelineObjectRoles>(response);
            //assert
            roleResponseObject.Roles.Should().Contain(role);
            roleResponse.StatusCode.Should().Be(HttpStatusCode.OK);


        }
        [Test, AutoData]
        public async Task Test_AddRoleToItem_given_ExistingRole_expect_success(Role role)
        {  // setup
            UserCredentials creds = await Login();
            TymelineObject tymelineObject = await getAll().ContinueWith(s => s.Result.RandomElement());
            var payload = new HttpTymelineObjectRolesIncrement { Role = role, tymelineObjectId = tymelineObject.Id };

            var setup = await _client.PostAsync($"https://localhost:5001/roles/additemrole", JsonContent.Create(payload));
            var setupresponse = await setup.Content.ReadAsStringAsync();

            var roleResponse = await _client.PostAsync($"https://localhost:5001/roles/additemrole", JsonContent.Create(payload));

            var response = await roleResponse.Content.ReadAsStringAsync();
            HttpTymelineObjectRoles roleResponseObject = JsonConvert.DeserializeObject<HttpTymelineObjectRoles>(response);
            //assert
            roleResponseObject.Roles.Should().Contain(role);
            roleResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        }


        [Test, AutoData]
        public async Task Test_RemoveRoleFromItem_given_AttachedRole_expect_OK_and_correctRoles(Role role, Role roleToRemove)
        { // setup
            UserCredentials creds = await Login();

            TymelineObject tymelineObject = await getAll().ContinueWith(s => s.Result.RandomElement());
            var payload = new HttpTymelineObjectRolesIncrement { Role = role, tymelineObjectId = tymelineObject.Id };
            var payloadToRemove = new HttpTymelineObjectRolesIncrement { Role = roleToRemove, tymelineObjectId = tymelineObject.Id };

            var setup = await _client.PostAsync($"https://localhost:5001/roles/additemrole", JsonContent.Create(payload));
            var setupToRemove = await _client.PostAsync($"https://localhost:5001/roles/additemrole", JsonContent.Create(payloadToRemove));
            setupToRemove.StatusCode.Should().Be(HttpStatusCode.OK);
            var response = await setupToRemove.Content.ReadAsStringAsync();
            HttpTymelineObjectRoles roleResponseObject = JsonConvert.DeserializeObject<HttpTymelineObjectRoles>(response);

            roleResponseObject.Roles.Should().Contain(role).And.Contain(roleToRemove);
            // given

            var assertResponse = await _client.PostAsync($"https://localhost:5001/roles/removeitemrole", JsonContent.Create(payloadToRemove));
            var assertString = await assertResponse.Content.ReadAsStringAsync();
            HttpTymelineObjectRoles assertObject = JsonConvert.DeserializeObject<HttpTymelineObjectRoles>(assertString);
            assertObject.Roles.Should().Contain(role).And.NotContain(roleToRemove);
            assertResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test, AutoData]
        public async Task Test_RemoveRoleFromItem_given_UnattachedRole_expect_OK_and_correctRoles(Role role, Role roleToRemove)
        { // setup
            UserCredentials creds = await Login();
            TymelineObject tymelineObject = await getAll().ContinueWith(s => s.Result.RandomElement());
            var payload = new HttpTymelineObjectRolesIncrement { Role = role, tymelineObjectId = tymelineObject.Id };
            var payloadToRemove = new HttpTymelineObjectRolesIncrement { Role = roleToRemove, tymelineObjectId = tymelineObject.Id };

            var setup = await _client.PostAsync($"https://localhost:5001/roles/additemrole", JsonContent.Create(payload));
            // given

            var assertResponse = await _client.PostAsync($"https://localhost:5001/roles/removeitemrole", JsonContent.Create(payloadToRemove));
            var assertString = await assertResponse.Content.ReadAsStringAsync();
            HttpTymelineObjectRoles assertObject = JsonConvert.DeserializeObject<HttpTymelineObjectRoles>(assertString);
            assertObject.Roles.Should().Contain(role).And.NotContain(roleToRemove);
            assertResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }


        [Test]
        public async Task Test_getItemRoles()
        {
            // setup
            UserCredentials creds = await Login();
        }


        [Test, AutoData]
        public async Task Test_GetRoles(Role ep)
        {
            // setup
            UserCredentials creds = await Login();
            HttpUserRole expectedPermission = new HttpUserRole(creds.Email, ep);
            Uri uriAddPermission = new Uri("https://localhost:5001/roles/addroletouser");
            await _client.PostAsync(uriAddPermission, JsonContent.Create(expectedPermission));
            var assertResponse = await _client.GetAsync($"https://localhost:5001/roles/getroles");
            var assertString = await assertResponse.Content.ReadAsStringAsync();
            List<Role> assertObject = JsonConvert.DeserializeObject<List<Role>>(assertString);
            assertObject.Should().Contain(ep);
            assertResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        }


        [Test, AutoData]
        [Repeat(2)]
        [Retry(2)]
        public async Task Test_AddRole_Expect_New_Role_To_Be_In_Roles(List<Role> roles)
        {
            UserCredentials creds = await Login();
            Uri uriAddPermission = new Uri("https://localhost:5001/roles/addrole");
            await roles.ForEachAsync(async role =>
                await _client.PostAsync(uriAddPermission, JsonContent.Create(role))
            );
            await Task.Delay(5);
            var assertResponse = await _client.GetAsync($"https://localhost:5001/roles/getroles");
            var assertString = await assertResponse.Content.ReadAsStringAsync();
            List<Role> assertObject = JsonConvert.DeserializeObject<List<Role>>(assertString);
            assertResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            assertObject.Should().Contain(roles);
        }

        [Test]
        public async Task Test_RemoveRole_Expect_Removed_Role_To_Not_Be_In_Roles()
        {
            UserCredentials creds = await Login();


            //setup
            var setupResponse = await _client.GetAsync($"https://localhost:5001/roles/getroles");
            var setupString = await setupResponse.Content.ReadAsStringAsync();
            List<Role> setupObject = JsonConvert.DeserializeObject<List<Role>>(setupString);
            int randomValue = TestUtil.RandomIntWithMax(setupObject.Count);
            Role role = setupObject[randomValue];
            Uri uriRemoveRole = new Uri("https://localhost:5001/roles/removerole");

            var setupRemove = await _client.PostAsync(uriRemoveRole, JsonContent.Create(role));

            //assert
            var assertResponse = await _client.GetAsync($"https://localhost:5001/roles/getroles");
            var assertString = await assertResponse.Content.ReadAsStringAsync();
            List<Role> assertObject = JsonConvert.DeserializeObject<List<Role>>(assertString);
            assertResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            assertObject.Should().NotContain(role);

        }


        [Test, AutoData]
        public async Task Test_RemoveRole_Expect_Removed_Role_To_Not_Be_In_Roles_For_Some_Email(int randomMail)
        {
            UserCredentials creds = await Login();


            //setup
            var setupResponse = await _client.GetAsync($"https://localhost:5001/roles/getroles");
            var setupString = await setupResponse.Content.ReadAsStringAsync();
            List<Role> setupObject = JsonConvert.DeserializeObject<List<Role>>(setupString);
            int randomValue = TestUtil.RandomIntWithMax(setupObject.Count);
            Role role = setupObject[randomValue];
            Uri uriRemoveRole = new Uri("https://localhost:5001/roles/removerole");

            var setupRemove = await _client.PostAsync(uriRemoveRole, JsonContent.Create(role));

            //assert
            var assertResponse = await _client.GetAsync($"https://localhost:5001/roles/getroles/email/test{randomMail % 100}@email.de");
            var assertString = await assertResponse.Content.ReadAsStringAsync();
            HttpUserRoles assertObject = JsonConvert.DeserializeObject<HttpUserRoles>(assertString);
            assertResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            assertObject.Roles.Should().NotContain(role);

        }




    }
}




