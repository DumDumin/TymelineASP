using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using NUnit.Framework;

using Tymeline.API.Daos;

namespace Tymeline.API.Tests
{
    public class AuthDaoTests : OneTimeSetUpAttribute
    {
        private AppSettings _configuration;
        ITymelineObjectDao _timelineObjectDao;
        IAuthDao _authDao;


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
            _configuration = GetApplicationConfiguration();
            IOptions<AppSettings> options = Options.Create<AppSettings>(_configuration);
            var s = new MySqlConnectionStringBuilder(options.Value.SqlConnection.MySqlConnectionString);
            var mySqlConnection = new MySqlConnection(s.ConnectionString);
            _timelineObjectDao = new TymelineObjectDaoMySql(mySqlConnection);
            _authDao = new AuthDao(mySqlConnection);
            TestUtil.setupDB(mySqlConnection);
            var items = TestUtil.prepopulateTymelineObjects(mySqlConnection);
            var users = TestUtil.prepopulateUser(mySqlConnection);
            var roles = TestUtil.prepopulateRoles(mySqlConnection);
            TestUtil.prepopulateUserRoles(mySqlConnection,roles,users);
            TestUtil.prepopulateItemRoles(mySqlConnection,roles,items);
            TestUtil.EnsureSuccessfullSetup(mySqlConnection);
        }



        [Test,AutoData]
        [Category("Sql")]
        public void Test_Register_New_Email_Expect_New_User(UserCredentials creds)
        {
            _authDao.Register(creds).Should().BeOfType<User>();
        }


        [Test]
        [Category("Sql")]
        public void Test_Register_AlreadyRegistered_Email_Expect_Argument_Exception()
        {
            var user = _authDao.GetUsers().First();
            IUserCredentials creds = new UserCredentials(user.Email,"asdf1234");
            
            Action action =() =>_authDao.Register(creds);
            action.Should().Throw<ArgumentException>();
        }

        [Test]
        [Category("Sql")]
        public void Test_GetUsers_Expect_List_Of_Users()
        {
            _authDao.GetUsers().Should().BeOfType<List<IUser>>();
        }

        [Test]
        [Category("Sql")]
        public void Test_GetUserByMail_With_Valid_Email_Expect_User()
        {
            var user = _authDao.GetUsers().First();
            _authDao.getUserByMail(user.Email).Should().BeOfType<User>();
        }

        [Test,AutoData]
        [Category("Sql")]
        public void Test_GetUserByMail_With_Invalid_Email_Expect_ArgumentException(string mail)
        {

            Action act = () =>_authDao.getUserByMail(mail);
            act.Should().Throw<ArgumentException>();
        }

        [Test,AutoData]
        [Category("Sql")]
        public void Test_ChangePassword_For_Existing_User_Expect_User(string password)
        {
            IUser user = _authDao.GetUsers().RandomElement();
            user = _authDao.ChangePassword(user,password);
            _authDao.getUserByMail(user.Email).verifyPassword(password);
            
        }

        [Test,AutoData]
        [Category("Sql")]
        public void Test_ChangePassword_For_Not_Existing_User_Expect_ArgumentException(User user,string password)
        {
            // var user = _authDao.GetUsers().RandomElement();
            Action act = () =>_authDao.ChangePassword(user,password);
            act.Should().Throw<ArgumentException>();
        }



        
    }
}