using Moq;
using System;
using NUnit.Framework;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Tymeline.API.Tests
{
    [TestFixture]
    [Category("HTTP")]
    public class AuthControllerUnitTests : OneTimeSetUpAttribute
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private Moq.Mock<IAuthService> _authService;
        private Mock<IDataRolesService> _dataRolesService;
        private UtilService _utilService;
        AppSettings _appSettings;
        Dictionary<string, IUser> userdict;
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _appSettings = Options.Create<AppSettings>(new AppSettings()).Value;

            _factory = new WebApplicationFactory<Startup>();
            _authService = new Moq.Mock<IAuthService>();
            _dataRolesService = new Mock<IDataRolesService>();
            _utilService = new UtilService();

            userdict = TestUtil.createUserDict();
            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IDataRolesService>(s => _dataRolesService.Object);
                    services.AddSingleton<IJwtService, JwtService>();
                    services.AddSingleton<IAuthService>(s => _authService.Object);
                    services.AddSingleton<UtilService>(s => _utilService);
                });
            }).CreateClient();
            _dataRolesService.Setup(s => s.GetUserRoles(It.IsAny<string>())).Returns((string email) => TestUtil.mockGetUserPermissions(email));
            _authService.Setup(s => s.GetUserRoles(It.IsAny<string>())).Returns((string email) => TestUtil.mockGetUserPermissions(email));
            _authService.Setup(s => s.Register(It.IsAny<IUserCredentials>())).Returns((IUserCredentials cc) => MockRegister(cc));
            _authService.Setup(s => s.getUsers()).Returns(() => MockGetUsers());
            _authService.Setup(s => s.Login(It.IsAny<IUserCredentials>())).Returns((UserCredentials cc) => MockLogin(cc));

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






        [Test]
        public async Task TestAuthRegister_with_new_email_return_201_with_content()
        {
            UserCredentials credentials = new UserCredentials("hunter@email.de", "hunter12");

            JsonContent content = JsonContent.Create(credentials);


            var response = await _client.PostAsync("https://localhost:5001/auth/register", content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(201, (int)statusCode);
            Assert.AreEqual(credentials.Email, JsonConvert.DeserializeObject<User>(responseString).Email);
            Assert.AreEqual(JsonConvert.SerializeObject(User.CredentialsToUser(credentials)), responseString);
        }


        [Test]
        public async Task TestAuthRegister_with_used_email_return_400_with_content()
        {
            UserCredentials credentials = new UserCredentials("test3@email.de", "hunter12");
            JsonContent content = JsonContent.Create(credentials);

            var response = await _client.PostAsync("https://localhost:5001/auth/register", content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(400, (int)statusCode);
            Assert.AreEqual("", responseString);

        }


        [Test]
        public async Task TestAuthRegister_with_invalid_email_return_400()
        {
            UserCredentials credentials = new UserCredentials("email.de", "hunter12");
            JsonContent content = JsonContent.Create(credentials);

            var response = await _client.PostAsync("https://localhost:5001/auth/register", content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(400, (int)statusCode);

        }


        [Test]
        public async Task TestUserList_expect_200_with_one_users()
        {
            var response = await _client.GetAsync("https://localhost:5001/auth/users");
            var responseObject = await response.Content.ReadFromJsonAsync<List<User>>();
            var statusCode = response.StatusCode;
            Assert.AreEqual(200, (int)statusCode);
            Assert.IsInstanceOf<List<User>>(responseObject);
        }




        [Test]
        public async Task TestUserLogin_with_registeredAccountAndCorrectCredentials_expectJWT()
        {
            UserCredentials credentials = new UserCredentials("test5@email.de", "hunter12");

            JsonContent content = JsonContent.Create(credentials);


            Uri uri = new Uri("https://localhost:5001/auth/login");
            var response = await _client.PostAsync(uri.AbsoluteUri, content);

            IEnumerable<string> cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
            var responseObject = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(200, (int)statusCode);
            Assert.IsNotEmpty(cookies);
        }


        [Test]
        public async Task TestUserLogin_with_registeredAccountAndIncorrectCredentials_expectError()
        {
            UserCredentials credentials = new UserCredentials("test5@email.de", "hunter13");

            JsonContent content = JsonContent.Create(credentials);
            Uri uri = new Uri("https://localhost:5001/auth/login");
            var response = await _client.PostAsync(uri.AbsoluteUri, content);

            IEnumerable<string> cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
            var responseObject = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(400, (int)statusCode);
        }

        [Test]
        public async Task TestUserLogin_with_unregisteredAccountAndIncorrectCredentials_expectError()
        {
            IUserCredentials credentials = new UserCredentials("test5@emailas.de", "hunter12");
            JsonContent content = JsonContent.Create(credentials);
            Uri uri = new Uri("https://localhost:5001/auth/login");
            var response = await _client.PostAsync(uri.AbsoluteUri, content);

            IEnumerable<string> cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
            var responseObject = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(400, (int)statusCode);
        }


        [Test]
        public async Task TestUserLogin_with_registeredAccount_Return_JWT_Test_JWT_Expect_Success()
        {
            await Login();
            Uri uriTest = new Uri("https://localhost:5001/auth/testjwt");
            var responseTest = await _client.GetAsync(uriTest.AbsoluteUri);
            var responseObject = await responseTest.Content.ReadAsStringAsync();
            var statusCode = responseTest.StatusCode;
            Assert.AreEqual(HttpStatusCode.OK, statusCode);
        }


        [Test]
        public async Task TestUserLogin_with_registeredAccount_Return_JWT_Test_Authentication_expect_Claims()
        {
            await Login();
            Uri uriTest = new Uri("https://localhost:5001/roles/userInfo");
            var responseTest = await _client.GetAsync(uriTest.AbsoluteUri);
            var responseObject = await responseTest.Content.ReadAsStringAsync();
            var statusCode = responseTest.StatusCode;
            Assert.AreEqual(HttpStatusCode.OK, statusCode);
        }



        [Test]
        public async Task Test_UserInfo_with_registeredAccount_Return_Permissions_for_User()
        {

            await Login();
            Uri uriTest = new Uri("https://localhost:5001/roles/userInfo");
            var responseTest = await _client.GetAsync(uriTest.AbsoluteUri);
            var responseObject = await responseTest.Content.ReadAsStringAsync();
            var parsedObject = JsonConvert.DeserializeObject<UserRoles>(responseObject);
            Assert.NotNull(parsedObject.Email);
            Assert.NotNull(parsedObject.Roles);
            Assert.AreEqual(TestUtil.mockGetUserPermissions("test5@email.de").Roles, parsedObject.Roles);


        }


        private IUser MockRegister(IUserCredentials credentials)
        {

            if (credentials.complete())
            {

                if (_utilService.IsValidEmail(credentials.Email).Equals(true))
                {

                    User user = User.CredentialsToUser(credentials);
                    if (userdict.ContainsKey(user.Email))
                    {
                        throw new ArgumentException();
                    }
                    else
                    {
                        userdict.Add(user.Email, user);
                        return user;
                    }

                }
                else
                {
                    throw new ArgumentException();
                }
            }
            else
            {
                throw new ArgumentException();
            }


        }

        private List<IUser> MockGetUsers()
        {
            return userdict.Values.ToList().Select(element => element).ToList();

        }

        private IUser MockLogin(UserCredentials credentials)
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

    }

}



