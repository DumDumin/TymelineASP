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


namespace Tymeline.API.Tests
{ 
    public class AuthControllerIntegrationTests : OneTimeSetUpAttribute
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private Moq.Mock<IAuthService> _authService;
        private UtilService _utilService;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _factory = new WebApplicationFactory<Startup>();
        }

        [SetUp]
        public void Setup()
        {
            _authService = new Moq.Mock<IAuthService>();
            _utilService = new UtilService();

            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services => 
                {
                    services.AddScoped<IAuthService>(s => _authService.Object);
                    services.AddSingleton<UtilService>(s => _utilService);
                });
            }).CreateClient();  
            
        }

        private Dictionary<int,IUser> createUserDict()
        {   
            Dictionary<int,IUser> users = new Dictionary<int, IUser>();
            for (int i = 2; i < 100; i++)
            {
                User user = new User($"test{i}@email.de",15000000);
                users.Add(user.UserId,user);
            }
            return users;
        }


        private IUser MockRegister(UserRegisterCredentials credentials){

            Dictionary<int,IUser> userdict = createUserDict();
            if(_utilService.IsValidEmail(credentials.Email).Equals(true)){

                IUser user = new User(credentials.Email,0);
                if (userdict.ContainsKey(credentials.Email.GetHashCode())){
                    throw new ArgumentException();
                }
                else {
                    return user;
                }

            }
            else{
                throw new FormatException();
            }
        }

        // private SigningCredentials MockGetSigningCredentials(){
        //     var sevenItems = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
        //     SymmetricSecurityKey key = new SymmetricSecurityKey(sevenItems);
        //     var credentials = new SigningCredentials(key,SecurityAlgorithms.RsaSha256);
        //     return credentials;
        // }


        [Test]
        public async Task TestAuthRegister_with_new_email_return_201_with_content(){
            UserRegisterCredentials credentials = new UserRegisterCredentials();
            credentials.CreatedAt = 1500000000;
            credentials.Email = "test@email.de";
            credentials.Password = "hunter12";
            JsonContent content =  JsonContent.Create(credentials);
            _authService.Setup(s => s.Register(It.IsAny<UserRegisterCredentials>())).Returns((UserRegisterCredentials cc) =>  MockRegister(cc));
            // _authService.Setup(s => s.GetSigningCredentials()).Returns(MockGetSigningCredentials());
            var response = await _client.PostAsync("https://localhost:5001/auth/register",content);
            var responseString = await response.Content.ReadFromJsonAsync<User>();
            var statusCode = response.StatusCode;
            Assert.AreEqual(credentials.Email,responseString.Mail);
            Assert.AreEqual(201,(int)statusCode);

        }


        [Test]
        public async Task TestAuthRegister_with_used_email_return_200_with_content(){
            UserRegisterCredentials credentials = new UserRegisterCredentials();
            credentials.CreatedAt = 1500000000;
            credentials.Email = "test3@email.de";
            credentials.Password = "hunter12";
            JsonContent content =  JsonContent.Create(credentials);
            _authService.Setup(s => s.Register(It.IsAny<UserRegisterCredentials>())).Returns((UserRegisterCredentials cc) =>  MockRegister(cc));
            // _authService.Setup(s => s.GetSigningCredentials()).Returns(MockGetSigningCredentials());
            var response = await _client.PostAsync("https://localhost:5001/auth/register",content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(200,(int)statusCode);
            Assert.AreEqual("",responseString);

        }


         [Test]
        public async Task TestAuthRegister_with_invalid_email_return_400(){
            UserRegisterCredentials credentials = new UserRegisterCredentials();
            credentials.CreatedAt = 1500000000;
            credentials.Email = "email.de";
            credentials.Password = "hunter12";
            JsonContent content =  JsonContent.Create(credentials);
            _authService.Setup(s => s.Register(It.IsAny<UserRegisterCredentials>())).Returns((UserRegisterCredentials cc) =>  MockRegister(cc));
            // _authService.Setup(s => s.GetSigningCredentials()).Returns(MockGetSigningCredentials());
            var response = await _client.PostAsync("https://localhost:5001/auth/register",content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(400,(int)statusCode);

        }



        // public async Task TestAuthRegister_with_already_registered_email_return_200_with_content()


        
        // }
        [Test]
        public async Task TestUserList_expect_200_with_one_users()
        {
            var response = await _client.GetAsync("https://localhost:5001/auth/users");
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(200,(int)statusCode);
        }
          
    }
  
}



