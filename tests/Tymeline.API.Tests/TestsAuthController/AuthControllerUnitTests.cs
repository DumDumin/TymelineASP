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

namespace Tymeline.API.Tests
{
    [TestFixture]
    [Category("HTTP")]
    public class AuthControllerUnitTests : OneTimeSetUpAttribute
    {
        private WebApplicationFactory<Startup> _factory;
        private string secret;
        private HttpClient _client;
        private Moq.Mock<IAuthService> _authService;

        private UtilService _utilService;
        AppSettings _appSettings;
        Dictionary<int,IUser> userdict;
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _appSettings = Options.Create<AppSettings>(new AppSettings()).Value;

            _factory = new WebApplicationFactory<Startup>();
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
            _authService.Setup(s => s.Register(It.IsAny<IUserCredentials>())).Returns((IUserCredentials cc) =>  MockRegister(cc));
            _authService.Setup(s => s.getUsers()).Returns(() =>  MockGetUsers());
            _authService.Setup(s => s.Login(It.IsAny<UserCredentials>())).Returns((UserCredentials cc) => MockLogin(cc));
            _authService.Setup(s => s.CreateJWT(It.IsAny<IUser>())).Returns((IUser user) => MockJWT(user));
            _authService.Setup(s => s.Login(It.IsAny<IUserCredentials>())).Returns((UserCredentials cc) => MockLogin(cc));
        }

        [SetUp]
        public void Setup()
        {
           userdict = createUserDict();
        }

        private Dictionary<int,IUser> createUserDict()
        {   
            var passwordHasher = new PasswordHasher();
            Dictionary<int,IUser> users = new Dictionary<int, IUser>();
            for (int i = 2; i < 100; i++)
            {
                User user = new User($"test{i}@email.de",passwordHasher.Hash("hunter12"));
                users.Add(user.UserId,user);
            }
            return users;
        }
        private IUser MockRegister(IUserCredentials credentials){

            if (credentials.complete())
            {
                
                if(_utilService.IsValidEmail(credentials.Email).Equals(true)){

                    User user = User.CredentialsToUser(credentials);
                    if (userdict.ContainsKey(user.UserId)){
                        return null;
                    }
                    else {
                        userdict.Add(user.UserId, user);
                        return user;
                    }

                }
                else{
                    return null;
                }
            }
            else
            {
                return null;
            }

            
        }

        private List<IUser> MockGetUsers()
        {
            return userdict.Values.ToList().Select(element => element).ToList();

        }

        private string MockJWT(IUser user) {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("id", user.UserId.ToString()),new Claim("name", user.Mail)}),
            
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private JwtSecurityToken MockJWTvalidate(string token){
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
            ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;
        var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
        return jwtToken;
    }


    private IUser MockLogin(UserCredentials credentials)
    {
        if (credentials.complete())
        {
            // check if user is registered
            if(userdict.ContainsKey(credentials.Email.GetHashCode())){
                if(
                MockPasswdCheck(credentials.Password, userdict[credentials.Email.GetHashCode()])){
                    return userdict[credentials.Email.GetHashCode()];
                }
            }
            return null;
        }
        else
        {
            return null;
        }
        
    }
    private bool MockPasswdCheck(string Password, IUser BaseUser){
        return BaseUser.verifyPassword(Password);
    }

    

        [Test]
        public async Task TestAuthRegister_with_new_email_return_201_with_content(){
            UserCredentials credentials = new UserCredentials("hunter@email.de","hunter12");

            JsonContent content =  JsonContent.Create(credentials);
       
        
            var response = await _client.PostAsync("https://localhost:5001/auth/register",content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(201,(int)statusCode);
            Assert.AreEqual(credentials.Email,JsonConvert.DeserializeObject<User>(responseString).Mail);
            Assert.AreEqual(JsonConvert.SerializeObject(User.CredentialsToUser(credentials)),responseString);

        }


        [Test]
        public async Task TestAuthRegister_with_used_email_return_400_with_content(){
            UserCredentials credentials = new UserCredentials("test3@email.de","hunter12");
            JsonContent content =  JsonContent.Create(credentials);
           
            var response = await _client.PostAsync("https://localhost:5001/auth/register",content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(400,(int)statusCode);
            Assert.AreEqual("",responseString);

        }


        [Test]
        public async Task TestAuthRegister_with_invalid_email_return_400(){
            UserCredentials credentials = new UserCredentials("email.de","hunter12");
            JsonContent content =  JsonContent.Create(credentials);
    
            var response = await _client.PostAsync("https://localhost:5001/auth/register",content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(400,(int)statusCode);

        }


        [Test]
        public async Task TestUserList_expect_200_with_one_users()
        {
            
            var response = await _client.GetAsync("https://localhost:5001/auth/users");
            var responseObject = await response.Content.ReadFromJsonAsync<List<User>>();
            var statusCode = response.StatusCode;
            Assert.AreEqual(200,(int)statusCode);
            Assert.IsInstanceOf<List<User>>(responseObject);
        }

       


        [Test]
        public async Task TestUserLogin_with_registeredAccountAndCorrectCredentials_expectJWT()
        {
            UserCredentials credentials = new UserCredentials("test5@email.de","hunter12");

            JsonContent content =  JsonContent.Create(credentials);
           
            
            Uri uri = new Uri("https://localhost:5001/auth/login");
            var response = await _client.PostAsync(uri.AbsoluteUri,content);
           
            IEnumerable<string> cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
            var responseObject = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(201,(int)statusCode);
            Assert.IsNotEmpty(cookies);
        }


        [Test]
        public async Task TestUserLogin_with_registeredAccountAndIncorrectCredentials_expectError()
        {
            UserCredentials credentials = new UserCredentials("test5@email.de","hunter13");

            JsonContent content =  JsonContent.Create(credentials);            
            Uri uri = new Uri("https://localhost:5001/auth/login");
            var response = await _client.PostAsync(uri.AbsoluteUri,content);
           
            IEnumerable<string> cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
            var responseObject = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(400,(int)statusCode);
        }

        [Test]
        public async Task TestUserLogin_with_unregisteredAccountAndIncorrectCredentials_expectError()
        {
            IUserCredentials credentials = new UserCredentials("test5@emailas.de", "hunter12");
            JsonContent content =  JsonContent.Create(credentials);
            Uri uri = new Uri("https://localhost:5001/auth/login");
            var response = await _client.PostAsync(uri.AbsoluteUri,content);
           
            IEnumerable<string> cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
            var responseObject = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(400,(int)statusCode);
        }
          
    }
  
}


