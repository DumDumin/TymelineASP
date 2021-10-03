using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
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
    public class TymelineControllerCreateUnitTest : OneTimeSetUpAttribute{

        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private Moq.Mock<ITymelineService> _tymelineService;
        private Moq.Mock<IAuthService> _authService;
        private Moq.Mock<IDataRolesService> _dataRolesService;
        private AppSettings _configuration;
         Dictionary<string,IUser> userdict;

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
                    services.AddSingleton(_configuration);
                });
            }).CreateClient();
            userdict = TestUtil.createUserDict();
            _tymelineService.Setup(s => s.Create(It.IsAny<TymelineObject>())).Returns((TymelineObject tO) => TestUtil.mockCreateTymelineObject(tO));
            _authService.Setup(s => s.GetUserRoles(It.IsAny<string>())).Returns((string email)=> TestUtil.mockGetUserPermissions(email));
            _authService.Setup(s => s.Login(It.IsAny<IUserCredentials>())).Returns((UserCredentials cc) => MockLogin(cc));
        }

    public IUser MockLogin(UserCredentials credentials)
    {
        if (credentials.complete())
        {
            // check if user is registered
            if(userdict.ContainsKey(credentials.Email)){
                return TestUtil.MockPasswdCheck(credentials.Password, userdict[credentials.Email]);
            }
            throw new ArgumentException();
        }
        else
        {
           throw new ArgumentException();
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
            _client.DefaultRequestHeaders.Add("Cookie",jwt);
            return credentials;
        }
        



        [Test]
        public async Task Test_TymelineCreate_With_New_Entry_Returns_New_Entry_And_201() {
            await Login();
            TymelineObject tymelineObject = new TymelineObject(189890,new Content("testContent"),10000000,false,false);
            Role r = new Role("item","personal");
            List<Role> roles  = new List<Role>();
            HttpTymelineObjectWithRoles to = new HttpTymelineObjectWithRoles{tymelineObject= tymelineObject, Roles = roles };
            JsonContent content =  JsonContent.Create(to);


            var response = await _client.PostAsync($"https://localhost:5001/tymeline/create",content);
            var responseString = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            Assert.AreEqual(HttpStatusCode.Created,statusCode);
        }


        [Test]
        public async Task Test_TymelineCreate_With_New_Entry_Returns_New_Entry_And_201_and_new_Id() {
            await Login();
            TymelineObject tymelineObject = new TymelineObject(){Content=new Content("testContent"), Length=10000000, Start=10000,CanChangeLength= false,CanMove= false};
            tymelineObject.Id.Should().BeNull();
            Role r = new Role("item","personal");
            List<Role> roles  = new List<Role>();
            HttpTymelineObjectWithRoles to = new HttpTymelineObjectWithRoles{tymelineObject= tymelineObject, Roles = roles };
            JsonContent content =  JsonContent.Create(to);


            var response = await _client.PostAsync($"https://localhost:5001/tymeline/create",content);
            var responseString = await response.Content.ReadAsStringAsync();
            var parsedObject = JsonConvert.DeserializeObject<TymelineObject>(responseString);
            var statusCode = response.StatusCode;
            Assert.AreEqual(HttpStatusCode.Created,statusCode);
            parsedObject.Id.Should().NotBeNull();

        }

    }
}