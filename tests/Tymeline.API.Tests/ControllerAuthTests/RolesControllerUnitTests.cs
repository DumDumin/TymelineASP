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
        private Dictionary<string, List<IPermission>> roles;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
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
                });
            }).CreateClient();
            
            _authService.Setup(s => s.Login(It.IsAny<IUserCredentials>())).Returns((UserCredentials cc) => MockLogin(cc));
            _authService.Setup(s => s.GetUserPermissions(It.IsAny<string>())).Returns((string email) => _rolesService.Object.GetUserPermissions(email));
            _authService.Setup(s => s.SetUserPermissions(It.IsAny<IUserPermissions>())).Callback((IUserPermissions permissions) => _rolesService.Object.SetUserPermissions(permissions));
            _authService.Setup(s => s.AddUserPermission(It.IsAny<string>(),It.IsAny<IPermission>())).Callback((string email, IPermission permission) => _rolesService.Object.AddUserPermission(email,permission));
            
            _rolesService.Setup(s => s.GetUserPermissions(It.IsAny<string>())).Returns((string email) => mockGetUserPermissions(email));
            _rolesService.Setup(s => s.SetUserPermissions(It.IsAny<IUserPermissions>())).Callback((IUserPermissions permissions) => mockSetRoles(permissions));
            _rolesService.Setup(s => s.AddUserPermission(It.IsAny<string>(),It.IsAny<IPermission>())).Callback((string email, IPermission permission) => MockAddRoles(email,permission));
        }


        [SetUp]
        public void Setup()
        {
            userdict = createUserDict();
            roles = createRoleDict();
        }

        [TearDown]
        public void TearDown(){
            _client.DefaultRequestHeaders.Clear();
        }

        void MockAddRoles(string email, IPermission permission){
            roles.TryGetValue(email,out var userRoles);
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

        private IUserPermissions mockGetUserPermissions(string email)
        {
            return new UserPermissions(email,roles[email]);
        }

        private Dictionary<string, IUser> createUserDict()
        {
            var passwordHasher = new PasswordHasher();
            Dictionary<string, IUser> users = new Dictionary<string, IUser>();
            for (int i = 2; i < 100; i++)
            {
                User user = new User($"test{i}@email.de", passwordHasher.Hash("hunter12"));
                users.Add(user.Mail, user);
            }
            return users;
        }

        private Dictionary<string, List<IPermission>> createRoleDict()
        {
            Dictionary<string, List<IPermission>> users = new Dictionary<string, List<IPermission>>();
            for (int i = 2; i < 100; i++)
            {
                var user = new UserPermissions($"test{i}@email.de", new List<IPermission>());
                users.Add(user.Email, user.Permissions);
            }
            return users;
        }

        private void mockSetRoles(IUserPermissions userPermissions){
            if(roles.ContainsKey(userPermissions.Email))
            {
                roles[userPermissions.Email] = userPermissions.Permissions;
            }
            else{
                roles.Add(userPermissions.Email,userPermissions.Permissions);
            }
        }

        private async Task Login()
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
        }

        private JwtSecurityToken ExtractTokenFromCookies(IEnumerable<string> c)
        {
            string webtoken = c.First(s => s.StartsWith("jwt"));
            webtoken = webtoken.Split(";").First(s => s.StartsWith("jwt")).Replace("jwt=", "");
            var parsedJwt = _jwtService.verifyJwt(webtoken);
            return parsedJwt;
        }

        [Test,AutoData]
        public async Task Test_UserInfo_with_registeredAccount_Set_Permissions_Return_New_Permissions_inUserInfo(List<Permission> permissionList)
        {
            await Login();
            JsonContent jsonList = JsonContent.Create(permissionList);

            Uri uriSetPermissions = new Uri("https://localhost:5001/auth/setpermissions");
            var setPermissions = await _client.PostAsync(uriSetPermissions, jsonList);
            var responseObjectsetPermissions = await setPermissions.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, setPermissions.StatusCode);

            Uri uriTest = new Uri("https://localhost:5001/auth/userInfo");
            var responseTest = await _client.GetAsync(uriTest.AbsoluteUri);
            var responseObject = await responseTest.Content.ReadAsStringAsync();
            var parsedObject = JsonConvert.DeserializeObject<UserPermissions>(responseObject);
            parsedObject.Permissions.Should().Contain(permissionList[0]).And.Contain(permissionList[1]).And.Contain(permissionList[2]);
        }

        [Test,AutoData]
        public async Task Test_UserInfo_with_registeredAccount_Set_Permissions_Return_New_Permissions_inJWT(List<Permission> testList)
        {
            await Login();

            JsonContent jsonList = JsonContent.Create(testList);

            Uri uriSetPermissions = new Uri("https://localhost:5001/auth/setpermissions");
            var setPermissions = await _client.PostAsync(uriSetPermissions, jsonList);
            var responseObjectsetPermissions = await setPermissions.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, setPermissions.StatusCode);
            IEnumerable<string> c = setPermissions.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
            JwtSecurityToken parsedJwt = ExtractTokenFromCookies(c);
            parsedJwt.Claims.ToList().Select(claim => new Permission(claim.Type, claim.Value)).ToList().Should().Contain(testList[0]);
        }

        [Test, AutoData]
        public async Task Test_Add_Role_With_Valid_Role(Permission expectedPermission){
            await Login();
            Uri uriAddPermission = new Uri("https://localhost:5001/auth/addpermission");
            var setPermissions = await _client.PostAsync(uriAddPermission,JsonContent.Create(expectedPermission));
            var responseObject = await setPermissions.Content.ReadAsStringAsync();
            IEnumerable<string> cookies = setPermissions.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
            JwtSecurityToken parsedJwt = ExtractTokenFromCookies(cookies);
            parsedJwt.Claims.ToList().Select(claim => new Permission(claim.Type, claim.Value)).ToList().Should().Contain(expectedPermission);


        }
    }

}



