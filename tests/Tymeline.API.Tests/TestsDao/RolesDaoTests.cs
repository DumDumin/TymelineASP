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
    [Category("Sql")]
    public class RolesDaoTests : OneTimeSetUpAttribute
    {
        private AppSettings _configuration;
        ITymelineObjectDao _timelineObjectDao;
        IAuthDao _authDao;

        public DataRolesDao _rolesDao { get; private set; }

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
            _rolesDao = new DataRolesDao(mySqlConnection,_authDao,_timelineObjectDao);
            TestUtil.setupDB(mySqlConnection);
            var items = TestUtil.prepopulateTymelineObjects(mySqlConnection);


            var roles = TestUtil.prepopulateRoles(mySqlConnection);
            var users = TestUtil.prepopulateUser(mySqlConnection);
            TestUtil.prepopulateUserRoles(mySqlConnection,roles,users);
            TestUtil.prepopulateItemRoles(mySqlConnection,roles,items);
        }


        private void setupDB(MySqlConnection connection){
            TestUtil.setupDB(connection);
        }

       


        [Test,AutoData]
        public void Test_AddRole_With_New_Role_Expect_Success(Role role){
            Action act = () => _rolesDao.AddRole(role);
            act.Should().NotThrow();
            _rolesDao.GetAllRoles().Should().Contain(role);
        }

        [Test]
        public void Test_AddRole_With_Existing_Role_Expect_ArgumentException(){
            // roles behave idempotent. 

            // no they fucking dont. Changed to expect throw
            IRole randomRole = _rolesDao.GetAllRoles().RandomElement();
            Action act = () =>  _rolesDao.AddRole(randomRole);
            act.Should().Throw<ArgumentException>();
        }


            [Test]
            public void Test_AddRoleToItem_With_Existing_Role_And_Existing_Item_Expect_Success(){
            TymelineObject randomItem = _timelineObjectDao.getAll().RandomElement();
            IRole randomRole =_rolesDao.GetAllRoles().RandomElement();
            
            _rolesDao.AddRoleToItem(randomRole,randomItem.Id).Roles.Should().Contain(randomRole);
            

        }

            [Test,AutoData]
            public void Test_AddRoleToItem_With_Existing_Role_And_Not_Existing_Item_Expect_Exception(TymelineObject fakeItem){
            
            IRole randomRole =_rolesDao.GetAllRoles().RandomElement();
            Action act = () => _rolesDao.AddRoleToItem(randomRole,fakeItem.Id);
            act.Should().Throw<ArgumentException>();
        }
            [Test,AutoData]
            public void Test_AddRoleToItem_With_Not_Existing_Role_And_Existing_Item_Expect_Exception(Role fakeRole){
            TymelineObject randomItem = _timelineObjectDao.getAll().RandomElement();
            Action act = () => _rolesDao.AddRoleToItem(fakeRole,randomItem.Id);
            act.Should().Throw<ArgumentException>();
        }
            [Test,AutoData]
            public void Test_AddRoleToItem_With_Not_Existing_Role_And_Not_Existing_Item_Expect_Exception(TymelineObject fakeItem,Role fakeRole){
            Action act = () => _rolesDao.AddRoleToItem(fakeRole,fakeItem.Id);
            act.Should().Throw<ArgumentException>();
        }
            [Test]
            public void Test_AddUserRole_With_Existing_Role_And_Existing_User_Expect_Success(){
            IUser randomUser = _authDao.GetUsers().RandomElement();
            IRole randomRole =_rolesDao.GetAllRoles().RandomElement();

            var userRoles = _rolesDao.AddUserRole(randomRole,randomUser.Email);
            userRoles.Roles.Should().Contain(randomRole);
        }
            [Test,AutoData]
            public void Test_AddUserRole_With_Existing_Role_And_Not_Existing_User_Expect_Exception(User fakeUser){
            IRole randomRole =_rolesDao.GetAllRoles().RandomElement();
            Action act = () => _rolesDao.AddUserRole(randomRole,fakeUser.Email);
            act.Should().Throw<ArgumentException>();
        }

             [Test,AutoData]
            public void Test_AddUserRole_With_Not_Existing_Role_And_Existing_User_Expect_Exception(Role fakeRole){
            IUser randomUser = _authDao.GetUsers().RandomElement();
            Action act = () => _rolesDao.AddUserRole(fakeRole,randomUser.Email);
            act.Should().Throw<ArgumentException>();
        }
             [Test,AutoData]
            public void Test_AddUserRole_With_Not_Existing_Role_And_Not_Existing_User_Expect_Exception(User fakeUser ,Role fakeRole){
            Action act = () => _rolesDao.AddUserRole(fakeRole,fakeUser.Email);
            act.Should().Throw<ArgumentException>();
        }

            [Test]
            public void Test_GetUserRoles_With_Existing_User_Expect_List_Of_Roles(){
                IUser randomUser = _authDao.GetUsers().RandomElement();
                var roles = _rolesDao.GetUserRoles(randomUser.Email);
                roles.Roles.Should().BeSubsetOf(_rolesDao.GetAllRoles());
            }

            [Test,AutoData]
            public void Test_GetUserRoles_With_Not_Existing_User_Expect_Exception(User fakeUser){
                Action act = () => _rolesDao.GetUserRoles(fakeUser.Email);
                act.Should().Throw<ArgumentException>();
            }

            public void Test_GetItemRoles_With_ExistingItem_Expect(){
                TymelineObject randomItem = _timelineObjectDao.getAll().RandomElement();
                _rolesDao.GetItemRoles(randomItem.Id);
            }
        

            [Test]
            public void Test_RemoveRoleFromItem_With_Existing_Item_And_Existing_Role_Expect_Success(){
                var randomItem = _timelineObjectDao.getAll().RandomElement();
                var randomRole =_rolesDao.GetItemRoles(randomItem.Id).Roles.RandomElement();
                _rolesDao.RemoveRoleFromItem(randomRole,randomItem.Id).Roles.Should().NotContain(randomRole);
            }


            [Test,AutoData]
            public void Test_RemoveRoleFromItem_With_Existing_Item_And_Not_Existing_Role_Expect_Success(Role fakeRole){
                var randomItem = _timelineObjectDao.getAll().RandomElement();
                _rolesDao.RemoveRoleFromItem(fakeRole,randomItem.Id).Roles.Should().NotContain(fakeRole);
                // act.Should().Throw<ArgumentException>();
            }

            [Test,AutoData]
            public void Test_RemoveRoleFromItem_With_Not_Existing_Item_And_Not_Existing_Role_Expect_ArgumentException(Role fakeRole, TymelineObject fakeItem){
                Action act = () => _rolesDao.RemoveRoleFromItem(fakeRole,fakeItem.Id);
                act.Should().Throw<ArgumentException>();
            }

            [Test]
            public void Test_RemoveUserRole_With_Existing_User_and_Existing_Role_Expect_Success(){
                var randomUser = _authDao.GetUsers().RandomElement();
                var randomRole = _rolesDao.GetUserRoles(randomUser.Email).Roles.RandomElement();
                _rolesDao.RemoveUserRole(randomRole, randomUser.Email).Roles.Should().NotContain(randomRole);
            }
            [Test,AutoData]
            public void Test_RemoveUserRole_With_Existing_User_and_Not_Existing_Role_Expect_Success(Role fakeRole){
                var randomUser = _authDao.GetUsers().RandomElement();
                _rolesDao.RemoveUserRole(fakeRole, randomUser.Email).Roles.Should().NotContain(fakeRole);
                // act.Should().Throw<ArgumentException>();
            }
            [Test,AutoData]
            public void Test_RemoveUserRole_With_Not_Existing_User_and_Not_Existing_Role_Expect_ArgumentException(User fakeUser, Role fakeRole){
                Action act = () =>_rolesDao.RemoveUserRole(fakeRole, fakeUser.Email);
                act.Should().Throw<ArgumentException>();
            }

            [Test]
            public void Test_RemoveRole_With_Existing_Role_Expect_User_To_Not_Possess_Role(){
                var randomUser = _authDao.GetUsers().RandomElement();
                var userRoles =_rolesDao.GetUserRoles(randomUser.Email);
                var randomRole = userRoles.Roles.RandomElement();
                _rolesDao.RemoveRole(randomRole);

            }
            [Test]
            public void Test_RemoveRole_With_Existing_Role_Expect_Item_To_Not_Possess_Role(){
                // _rolesDao.RemoveRole()
            }











        
    }
}