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
            TestUtil.prepopulateTymelineObjects(mySqlConnection);


            var roles = prepopulateRoles(mySqlConnection);
            var users = prepopulateUser(mySqlConnection);
            prepopulateUserRoles(mySqlConnection,roles,users);
        }


        private void setupDB(MySqlConnection connection){
            TestUtil.setupDB(connection);
        }

        private  List<IRole> prepopulateRoles(MySqlConnection connection){
            Fixture fix = new Fixture();
            fix.Customizations.Add(
                new TypeRelay(
                    typeof(IRole),
                    typeof(Role)));
                    ;
            List<IRole> expectRole = fix.CreateMany<IRole>(50).ToList();
            expectRole.ForEach(role => {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT into Roles(role_id,role_name,role_value) values(@role_id,@role_name,@role_value)";

                command.Parameters.AddWithValue("@role_id",role.RoleId);
                command.Parameters.AddWithValue("@role_name",role.Type);
                command.Parameters.AddWithValue("@role_value",role.Value);
                command.ExecuteNonQuery();
                connection.Close();
            });
            return expectRole;
        }


        private List<IUserCredentials> prepopulateUser(MySqlConnection connection){
            Fixture fix = new Fixture();
             fix.Customizations.Add(
                new TypeRelay(
                    typeof(IUserCredentials),
                    typeof(UserCredentials)));
                    ;
            List<IUserCredentials> expectedUsercreds = fix.CreateMany<IUserCredentials>(50).ToList();
            expectedUsercreds.ForEach(creds => {
                connection.Open();
                creds.Password = "asdf1234";
                var command = connection.CreateCommand();
                var user = User.CredentialsToUser(creds).ToDaoUser();
                command.CommandText = "INSERT into Users(user_id,email,password) values(@user_id,@email,@password)";

                command.Parameters.AddWithValue("@user_id",user.user_id);
                command.Parameters.AddWithValue("@email",user.email);
                command.Parameters.AddWithValue("@password",user.password);
                command.ExecuteNonQuery();
                connection.Close();
            });
            return expectedUsercreds;
        }


        private void prepopulateUserRoles(MySqlConnection connection, List<IRole> roles, List<IUserCredentials> credentials){

            connection.Open();
            credentials.ForEach(creds => {
                var user = User.CredentialsToUser(creds).ToDaoUser();
                var userRoles = roles.RandomElements(3);
                userRoles.ForEach(role => {
                    var command = new MySqlCommand();
                    command.Connection = connection;
                    command.CommandText = "INSERT into UserRoleRelation(user_fk,role_fk) values(@user_fk,@role_fk)";
                    command.Parameters.AddWithValue("@user_fk",user.user_id);
                    command.Parameters.AddWithValue("@role_fk",role.RoleId);
                    command.ExecuteNonQuery();
                });
            });
            connection.Close();
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