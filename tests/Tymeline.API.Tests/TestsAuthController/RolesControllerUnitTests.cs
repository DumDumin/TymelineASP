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

namespace Tymeline.API.Tests
{
    [TestFixture]
    [Category("HTTP")]
    public class RolesControllerUnitTests : OneTimeSetUpAttribute
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private Moq.Mock<IRolesService> _rolesService;


        private UtilService _utilService;
        AppSettings _appSettings;
        Dictionary<string,IUser> userdict;
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _appSettings = Options.Create<AppSettings>(new AppSettings()).Value;

            _factory = new WebApplicationFactory<Startup>();
            _rolesService = new Moq.Mock<IRolesService>();
            _utilService = new UtilService();


            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services => 
                {   
                    services.AddSingleton<IJwtService,JwtService>();      
                    services.AddScoped<IRolesService>(s => _rolesService.Object);
                    services.AddSingleton<UtilService>(s => _utilService);
                });
            }).CreateClient();  
            
        }

        [SetUp]
        public void Setup()
        {
           userdict = createUserDict();
        }

        private IUserPermissions mockGetUserPermissions(string email){
            var UserPermissions = new UserPermissions(email, new List<IPermission>());
            UserPermissions.Permissions.Add(new Permission("test","value"));
            return UserPermissions;
        }

        private Dictionary<string,IUser> createUserDict()
        {   
            var passwordHasher = new PasswordHasher();
            Dictionary<string,IUser> users = new Dictionary<string, IUser>();
            for (int i = 2; i < 100; i++)
            {
                User user = new User($"test{i}@email.de",passwordHasher.Hash("hunter12"));
                users.Add(user.Mail,user);
            }
            return users;
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


    
          
    }
  
}



