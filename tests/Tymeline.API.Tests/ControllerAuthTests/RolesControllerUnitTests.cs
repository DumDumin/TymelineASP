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
        Dictionary<string, IUser> userdict;
        private Dictionary<string, List<IRole>> roles;
        private List<TymelineObject> tymelineList;

        private Dictionary<string,List<IRole>> tymelineObjectRoles;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {

            tymelineList = TestUtil.setupTymelineList();
            tymelineObjectRoles = TestUtil.setupRoles(tymelineList);


            _appSettingsOptions = Options.Create<AppSettings>(new AppSettings());
            _appSettings = _appSettingsOptions.Value;
            _factory = new WebApplicationFactory<Startup>();
            _authService = new Moq.Mock<IAuthService>();
            _utilService = new UtilService();
            _rolesService = new Mock<IDataRolesService>();
            _jwtService = new JwtService(_rolesService.Object, _appSettingsOptions);


            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IJwtService>(s => _jwtService);
                    services.AddSingleton<IAuthService>(s => _authService.Object);
                    services.AddSingleton<UtilService>(s => _utilService);
                    services.AddSingleton<IDataRolesService>(s => _rolesService.Object);
                });
            }).CreateClient();

            _authService.Setup(s => s.Login(It.IsAny<IUserCredentials>())).Returns((UserCredentials cc) => MockLogin(cc));

            _rolesService.Setup(s => s.GetUserRoles(It.IsAny<string>())).Returns((string email) => mockGetUserPermissions(email));
            _rolesService.Setup(s => s.SetUserRoles(It.IsAny<IUserRoles>())).Callback((IUserRoles permissions) => mockSetRoles(permissions));
            _rolesService.Setup(s => s.AddUserRole(It.IsAny<string>(), It.IsAny<IRole>())).Callback((string email, IRole permission) => MockAddRoles(email, permission));
            _rolesService.Setup(s => s.AddUserRole(It.IsAny<IUserRole>())).Callback((IUserRole userRole) => MockAddRoles(userRole.Email, userRole.Roles));
            _rolesService.Setup(s => s.RemoveUserRole(It.IsAny<string>(), It.IsAny<IRole>())).Callback((string Email, IRole role) => MockRemoveUserRole(Email, role));
            _rolesService.Setup(s => s.RemoveUserRole(It.IsAny<IUserRole>())).Callback((IUserRole userRole) => MockRemoveUserRole(userRole.Email, userRole.Roles));
            _rolesService.Setup(s => s.AddRoleToItem(It.IsAny<IRole>(),It.IsAny<TymelineObject>())).Returns((IRole role, TymelineObject to) => mockAddRoleToItem(role,to));
            _rolesService.Setup(s => s.RemoveRoleFromItem(It.IsAny<IRole>(),It.IsAny<TymelineObject>())).Returns((IRole role, TymelineObject to) => mockRemoveRoleFromItem(role,to));
            _authService.Setup(s => s.getUsers()).Returns(() => MockGetUsers());
        }


        List<IRole> mockAddRoleToItem(IRole role, TymelineObject to){
            tymelineObjectRoles.TryGetValue(to.Id,out List<IRole> roles);
            if(!roles.Contains(role)){
                // only add each role once!
                roles.Add(role);
            }
            return roles;
        }

        List<IRole> mockRemoveRoleFromItem(IRole role, TymelineObject to){
            tymelineObjectRoles.TryGetValue(to.Id,out List<IRole> roles);
            roles.Remove(role);
            return roles;
        }


        [SetUp]
        public void Setup()
        {
            userdict = createUserDict();
            roles = createRoleDict();
        }

        [TearDown]
        public void TearDown()
        {
            _client.DefaultRequestHeaders.Clear();
        }

        private void MockRemoveUserRole(string Email, IRole Role)
        {
            roles.TryGetValue(Email, out var userRoles);
            userRoles.Remove(Role);
        }

        private List<IUser> MockGetUsers()
        {
            return userdict.Values.ToList().Select(element => element).ToList();

        }



        void MockAddRoles(string email, IRole permission)
        {
            roles.TryGetValue(email, out var userRoles);
            userRoles.Add(permission);
        }

        private IUser MockLogin(UserCredentials credentials)
        {
            if (credentials.complete())
            {
                // check if user is registered
                if (userdict.ContainsKey(credentials.Email))
                {
                    return MockPasswdCheck(credentials.Password, userdict[credentials.Email]);
                }
                throw new ArgumentException();
            }
            else
            {
                throw new ArgumentException();
            }
        }
        private IUser MockPasswdCheck(string Password, IUser BaseUser)
        {
            return BaseUser.verifyPassword(Password);
        }

        private IUserRoles mockGetUserPermissions(string email)
        {
            return new UserRoles(email, roles[email]);
        }

        private Dictionary<string, IUser> createUserDict()
        {
            var passwordHasher = new PasswordHasher();
            Dictionary<string, IUser> users = new Dictionary<string, IUser>();
            for (int i = 2; i < 100; i++)
            {
                User user = new User($"test{i}@email.de", passwordHasher.Hash("hunter12"));
                users.Add(user.Email, user);
            }
            return users;
        }

        private Dictionary<string, List<IRole>> createRoleDict()
        {
            Dictionary<string, List<IRole>> users = new Dictionary<string, List<IRole>>();
            for (int i = 2; i < 100; i++)
            {
                var user = new UserRoles($"test{i}@email.de", new List<IRole>());
                users.Add(user.Email, user.Permissions);
            }
            return users;
        }

        private void mockSetRoles(IUserRoles userPermissions)
        {
            if (roles.ContainsKey(userPermissions.Email))
            {
                roles[userPermissions.Email] = userPermissions.Permissions;
            }
            else
            {
                roles.Add(userPermissions.Email, userPermissions.Permissions);
            }
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

        private JwtSecurityToken ExtractTokenFromCookies(IEnumerable<string> c)
        {
            string webtoken = c.First(s => s.StartsWith("jwt"));
            webtoken = webtoken.Split(";").First(s => s.StartsWith("jwt")).Replace("jwt=", "");
            var parsedJwt = _jwtService.verifyJwt(webtoken);
            return parsedJwt;
        }

        [Test, AutoData]
        public async Task Test_UserInfo_with_registeredAccount_Set_Permissions_Return_New_Permissions_inUserInfo(HttpUserPermissions userPermissions)
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
            parsedObject.Permissions.Should().Contain(userPermissions.Permissions[0]).And.Contain(userPermissions.Permissions[1]).And.Contain(userPermissions.Permissions[2]);
        }

        [Test, AutoData]
        public async Task Test_UserInfo_with_registeredAccount_Set_Role_Return_New_Role_inJWT(List<Role> testList)
        {

            //setup
            testList[0].Type = "Frontend";
            UserCredentials creds = await Login();

            //given
            HttpUserPermissions expectedPermission = new HttpUserPermissions(creds.Email, testList);
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
            HttpUserPermissions expectedPermission = new HttpUserPermissions(creds.Email, testList);
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
            HttpUserPermission expectedPermission = new HttpUserPermission(creds.Email, ep);
            Uri uriAddPermission = new Uri("https://localhost:5001/roles/addrole");
            var setPermissions = await _client.PostAsync(uriAddPermission, JsonContent.Create(expectedPermission));
            var responseObject = await setPermissions.Content.ReadAsStringAsync();
            IEnumerable<string> cookies = setPermissions.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
            JwtSecurityToken parsedJwt = ExtractTokenFromCookies(cookies);
            //assert
            parsedJwt.Claims.ToList().Select(claim => new Role(claim.Type, claim.Value)).ToList().Should().Contain(expectedPermission.Permission.As<Role>());
        }





        [Test, AutoData]

        public async Task Test_Get_Roles(Role ep)
        {
            UserCredentials creds = await Login();

            User user = await GetRandomUser();

            HttpUserPermission expectedPermission = new HttpUserPermission(user.Email, ep);
            Uri uriAddPermission = new Uri("https://localhost:5001/roles/addrole");
            var setPermissions = await _client.PostAsync(uriAddPermission, JsonContent.Create(expectedPermission));


            var roleResponse = await _client.GetAsync($"https://localhost:5001/roles/getroles/{user.Email}");

            var s = await roleResponse.Content.ReadAsStringAsync();
            var roleResponseObject = JsonConvert.DeserializeObject<UserRoles>(s);
            var roleStatusCode = roleResponse.StatusCode;

            roleResponseObject.Email.Should().Be(user.Email);
            roleResponseObject.Permissions.Should().BeOfType<List<IRole>>();
            roleResponseObject.Permissions.Should().Contain(ep);

        }


        [Test, AutoData]
        public async Task Test_Get_Roles_Set(List<Role> testList)
        {
            Uri uriSetPermissions = new Uri("https://localhost:5001/roles/setroles");
            //setup
            UserCredentials creds = await Login();

            User user = await GetRandomUser();

            //given

            HttpUserPermissions expectedPermission = new HttpUserPermissions(user.Email, testList); //create new HttpUserPermission
            var setPermissions = await _client.PostAsync(uriSetPermissions, JsonContent.Create(expectedPermission));
            var roleResponse = await _client.GetAsync($"https://localhost:5001/roles/getroles/{user.Email}");

            var s = await roleResponse.Content.ReadAsStringAsync();
            var roleResponseObject = JsonConvert.DeserializeObject<UserRoles>(s);
            var roleStatusCode = roleResponse.StatusCode;

            roleResponseObject.Email.Should().Be(user.Email);
            roleResponseObject.Permissions.Should().BeOfType<List<IRole>>().And.Contain(testList);

        }

        [Test, AutoData]
        public async Task Test_Remove_Role_With_Valid_Role(Role ep, Role ep_keep)
        {
            // setup
            UserCredentials creds = await Login();
            User user = await GetRandomUser();
            HttpUserPermission expectedPermission = new HttpUserPermission(user.Email, ep);
            HttpUserPermission expectedPermission_keep = new HttpUserPermission(user.Email, ep_keep);
            Uri uriAddPermission = new Uri("https://localhost:5001/roles/addrole");

            //given

            await _client.PostAsync(uriAddPermission, JsonContent.Create(expectedPermission));
            await _client.PostAsync(uriAddPermission, JsonContent.Create(expectedPermission_keep));
            Uri uriRemovePermission = new Uri("https://localhost:5001/roles/removerole");
            await _client.PostAsync(uriRemovePermission, JsonContent.Create(expectedPermission));

            //assert

            var roleResponse = await _client.GetAsync($"https://localhost:5001/roles/getroles/{user.Email}");

            var s = await roleResponse.Content.ReadAsStringAsync();
            var roleResponseObject = JsonConvert.DeserializeObject<UserRoles>(s);
            var roleStatusCode = roleResponse.StatusCode;

            roleResponseObject.Email.Should().Be(user.Email);
            roleResponseObject.Permissions.Should().BeOfType<List<IRole>>().And.Contain(ep_keep).And.NotContain(ep);


        }

        [Test,AutoData]
        public async Task Test_AddRoleToItem_given_NewRole_expect_NewRoleCreation(Role role)
        {  // setup
            UserCredentials creds = await Login();
            TymelineObject tymelineObject = tymelineList[TestUtil.RandomIntWithMax(tymelineList.Count)];
            var payload = new HttpTymelineObjectRolesIncrement{Role= role, tymelineObject = tymelineObject};

            var roleResponse = await _client.PostAsync($"https://localhost:5001/roles/additemrole",JsonContent.Create(payload));

            var response = await roleResponse.Content.ReadAsStringAsync();
            HttpTymelineObjectRoles roleResponseObject = JsonConvert.DeserializeObject<HttpTymelineObjectRoles>(response);
            //assert
            roleResponseObject.Roles.Should().Contain(role);
            roleResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            
        }
        [Test,AutoData]
        public async Task Test_AddRoleToItem_given_ExistingRole_expect_success(Role role)
        {  // setup
            UserCredentials creds = await Login();
            TymelineObject tymelineObject = tymelineList[TestUtil.RandomIntWithMax(tymelineList.Count)];
            var payload = new HttpTymelineObjectRolesIncrement{Role= role, tymelineObject = tymelineObject};

            var setup = await _client.PostAsync($"https://localhost:5001/roles/additemrole",JsonContent.Create(payload));
            var setupresponse = await setup.Content.ReadAsStringAsync();
         
          


            var roleResponse = await _client.PostAsync($"https://localhost:5001/roles/additemrole",JsonContent.Create(payload));

            var response = await roleResponse.Content.ReadAsStringAsync();
            HttpTymelineObjectRoles roleResponseObject = JsonConvert.DeserializeObject<HttpTymelineObjectRoles>(response);
            //assert
            roleResponseObject.Roles.Should().Contain(role).And.HaveCount(1);
            roleResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        }
       

        [Test,AutoData]
        public async Task Test_RemoveRoleFromItem_given_AttachedRole_expect_OK_and_correctRoles(Role role,Role roleToRemove)
        { // setup
            UserCredentials creds = await Login();

            TymelineObject tymelineObject = tymelineList[TestUtil.RandomIntWithMax(tymelineList.Count)];
            var payload = new HttpTymelineObjectRolesIncrement{Role= role, tymelineObject = tymelineObject};
            var payloadToRemove = new HttpTymelineObjectRolesIncrement{Role= roleToRemove, tymelineObject = tymelineObject};

            var setup = await _client.PostAsync($"https://localhost:5001/roles/additemrole",JsonContent.Create(payload));
            var setupToRemove = await _client.PostAsync($"https://localhost:5001/roles/additemrole",JsonContent.Create(payloadToRemove));
            var response = await setupToRemove.Content.ReadAsStringAsync();
            HttpTymelineObjectRoles roleResponseObject = JsonConvert.DeserializeObject<HttpTymelineObjectRoles>(response);

            roleResponseObject.Roles.Should().Contain(role).And.Contain(roleToRemove).And.HaveCount(2);
            // given

            var assertResponse = await _client.PostAsync($"https://localhost:5001/roles/removeitemrole",JsonContent.Create(payloadToRemove));
            var assertString = await assertResponse.Content.ReadAsStringAsync();
            HttpTymelineObjectRoles assertObject = JsonConvert.DeserializeObject<HttpTymelineObjectRoles>(assertString);
            assertObject.Roles.Should().Contain(role).And.NotContain(roleToRemove);
            assertResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test,AutoData]
        public async Task Test_RemoveRoleFromItem_given_UnattachedRole_expect_OK_and_correctRoles(Role role,Role roleToRemove)
        { // setup
            UserCredentials creds = await Login();
            TymelineObject tymelineObject = tymelineList[TestUtil.RandomIntWithMax(tymelineList.Count)];
            var payload = new HttpTymelineObjectRolesIncrement{Role= role, tymelineObject = tymelineObject};
            var payloadToRemove = new HttpTymelineObjectRolesIncrement{Role= roleToRemove, tymelineObject = tymelineObject};

            var setup = await _client.PostAsync($"https://localhost:5001/roles/additemrole",JsonContent.Create(payload));           
            // given

            var assertResponse = await _client.PostAsync($"https://localhost:5001/roles/removeitemrole",JsonContent.Create(payloadToRemove));
            var assertString = await assertResponse.Content.ReadAsStringAsync();
            HttpTymelineObjectRoles assertObject = JsonConvert.DeserializeObject<HttpTymelineObjectRoles>(assertString);
            assertObject.Roles.Should().Contain(role).And.NotContain(roleToRemove);
            assertResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }


        [Test]
        public async Task Test_getItemRoles(){
            // setup
            UserCredentials creds = await Login();
            }




    }
}




