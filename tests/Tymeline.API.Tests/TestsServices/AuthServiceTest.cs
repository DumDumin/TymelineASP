using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Tymeline.API.Controllers;

namespace Tymeline.API.Tests
{
    public class AuthServiceTest : OneTimeSetUpAttribute
    {
        IAuthService _authService;
        Moq.Mock<IAuthDao> _authDao;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            IOptions<AppSettings> appSettings = Options.Create<AppSettings>(new AppSettings());
            _authDao = new Moq.Mock<IAuthDao>();
            _authService = new AuthService(_authDao.Object,appSettings);
        }


        [Test]
        public void Test_Login_Given_Valid_Credentials_Expect_IUser(){
            UserCredentials credentials = new UserCredentials();
            
            _authService.Login()
        }

    
        [Test]
        public void Test_Register_Given_Valid_Credentials_Expect_IUser(){
            _authService.Register()
        }


    }
}