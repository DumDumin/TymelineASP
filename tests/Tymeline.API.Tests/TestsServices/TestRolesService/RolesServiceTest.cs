using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using Tymeline.API.Controllers;

namespace Tymeline.API.Tests
{
    [Category("Service")]
    public class RolesServiceTest : OneTimeSetUpAttribute
    {
        IAuthService _authService;
        private IDataRolesService _rolesService;
        Moq.Mock<IAuthDao> _authDao;
        public Mock<IDataRolesDao> _rolesDao { get; private set; }




        UtilService _utilService;
        IJwtService _jwtService;
        AppSettings _appSettings;

        Dictionary<string, IUser> userdict;
        public List<TymelineObject> tymelineList { get; private set; }
        public List<IRole> roleList { get; private set; }
        public Dictionary<string, List<IRole>> tymelineObjectRoles { get; private set; }
        public Dictionary<string, List<IRole>> userRoles { get; private set; }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var _appSettingsOptions = Options.Create<AppSettings>(new AppSettings());
            tymelineList = TestUtil.setupTymelineList();
            roleList = TestUtil.CreateRoleList();

            tymelineList = TestUtil.setupTymelineList();
            tymelineObjectRoles = TestUtil.setupRoles(tymelineList, roleList);

            userdict = TestUtil.createUserDict();
            userRoles = TestUtil.createRoleDict(userdict, roleList);

            tymelineObjectRoles = TestUtil.setupRoles(tymelineList, roleList);
            _appSettings = _appSettingsOptions.Value;
            _authDao = new Moq.Mock<IAuthDao>();
            _rolesDao = new Moq.Mock<IDataRolesDao>();
            _utilService = new UtilService();
            _rolesService = new DataRolesService(_rolesDao.Object);
            _authService = new AuthService(_authDao.Object, _utilService, _rolesService, _appSettingsOptions);
            _jwtService = new JwtService(_authService, _appSettingsOptions);
            _authDao.Setup(s => s.getUserByMail(It.IsAny<string>())).Returns((string mail) => MockGetUserByMail(mail));
            _authDao.Setup(s => s.GetUsers()).Returns(() => MockGetUser());
            _authDao.Setup(s => s.Register(It.IsAny<IUserCredentials>())).Returns((IUserCredentials user) => MockRegister(user));
            _authDao.Setup(s => s.RemoveUser(It.IsAny<IUser>())).Callback((IUser user) => MockRemoveUser(user));
            _authDao.Setup(s => s.ChangePassword(It.IsAny<IUser>(), It.IsAny<string>())).Returns((IUser user, string password) => MockChangePassword(user, password));


            _rolesDao.Setup(s => s.SetUserRoles(It.IsAny<IUserRoles>())).Callback((IUserRoles s) => mockSetUserRoles(s));
            _rolesDao.Setup(s => s.RemoveUserFromRole(It.IsAny<IRole>(), It.IsAny<string>())).Returns((IRole r, string s) => mockRemoveUserRole(r, s));
            _rolesDao.Setup(s => s.RemoveRoleFromItem(It.IsAny<IRole>(), It.IsAny<string>())).Returns((IRole r, string i) => mockRemoveRoleFromItem(r, i));
            _rolesDao.Setup(s => s.RemoveRole(It.IsAny<IRole>())).Callback((IRole s) => mockRemoveRole(s));
            _rolesDao.Setup(s => s.GetUserRoles(It.IsAny<string>())).Returns((string s) => mockGetUserRoles(s));
            _rolesDao.Setup(s => s.GetItemRoles(It.IsAny<string>())).Returns((string s) => mockGetItemRoles(s));
            _rolesDao.Setup(s => s.GetAllRoles()).Returns(() => mockGetAllRoles());
            _rolesDao.Setup(s => s.AddUserRole(It.IsAny<IRole>(), It.IsAny<string>())).Returns((IRole r, string user) => mockAddUserRole(r, user));
            _rolesDao.Setup(s => s.AddRoleToItem(It.IsAny<IRole>(), It.IsAny<string>())).Returns((IRole r, string item) => mockAddItemRole(r, item));
            _rolesDao.Setup(s => s.AddRole(It.IsAny<IRole>())).Callback((IRole s) => mockAddRole(s));

        
        }

        private void mockAddRole(IRole s)
        {
            if (!roleList.Contains(s))
            {
                roleList.Add(s);
            }
        }

        private IUserRoles mockRemoveUserRole(IRole r, string s)
        {


            if (userRoles.TryGetValue(s, out var RoleList))
            {

                RoleList.Remove(r);
                
                return new UserRoles(s,RoleList);
            }
            else
            {
                throw new ArgumentException();
            }

        }

        private ITymelineObjectRoles mockRemoveRoleFromItem(IRole r, string i)
        {

            if (tymelineObjectRoles.TryGetValue(i, out var RoleList))
            {
                RoleList.Remove(r);
                return new TymelineObjectRoles(i,RoleList);

            }
            else
            {
                throw new ArgumentException();

            }
        }

        private void mockRemoveRole(IRole role)
        {
            if (roleList.Contains(role))
            {

                var relevantTymelineObjects = tymelineObjectRoles.Where(s => s.Value.Contains(role)).ToList().Select(kvpair => kvpair.Key).ToList();
                relevantTymelineObjects.ForEach(to => mockRemoveRoleFromItem(role, to));
                var relevantUsers = userRoles.Where(s => s.Value.Contains(role)).ToList().Select(kvpair => kvpair.Key).ToList();
                relevantUsers.ForEach(user => mockRemoveUserRole(role, user));
                roleList.Remove(role);
            }
        }

        private IUserRoles mockGetUserRoles(string s)
        {

            if (userRoles.TryGetValue(s, out var RoleList))
            {
                IUserRoles user = new UserRoles(s, RoleList);
                return user;

            }
            else
            {
                throw new ArgumentException();

            }

        }

        private ITymelineObjectRoles mockGetItemRoles(string s)
        {


            if (tymelineObjectRoles.TryGetValue(s, out var RoleList))
            {
                ITymelineObjectRoles item = new TymelineObjectRoles(s, RoleList);
                return item;
            }
            else
            {
                throw new ArgumentException();

            }



        }

        private List<IRole> mockGetAllRoles()
        {
            return roleList;
        }


        private void mockSetUserRoles(IUserRoles s)
        {
            if (userRoles.TryGetValue(s.Email, out var RoleList))
            {
                roleList.Clear();
                s.Roles.ForEach(role => RoleList.Add(role));

            }
            else
            {
                throw new ArgumentException();
            }
        }

        private IUserRoles mockAddUserRole(IRole r, string user)
        {

            if (userRoles.TryGetValue(user, out var RoleList))
            {
                if (!RoleList.Contains(r))
                {
                    RoleList.Add(r);
                }
                return new UserRoles(user,RoleList);
            }
            else
            {
                throw new ArgumentException();

            }

        }

        private ITymelineObjectRoles mockAddItemRole(IRole r, string item)
        {

            if (tymelineObjectRoles.TryGetValue(item, out var RoleList))
            {
                if (!RoleList.Contains(r))
                {
                    RoleList.Add(r);
                }
                return new TymelineObjectRoles(item,RoleList);
            }
            else
            {
                throw new ArgumentException();

            }

        }


        [SetUp]
        public void Setup()
        {
            
        }



        private Dictionary<string, IUser> createUserDict()
        {
            var passwordHasher = new PasswordHasher();
            Dictionary<string, IUser> users = new Dictionary<string, IUser>();
            for (int i = 2; i < 100; i++)
            {
                User user = new User($"test{i}@email.de", passwordHasher.Hash("hunter12"));
                users.Add(user.Email, user);
            }
            return users;
        }

        IUser MockChangePassword(IUser user, string password)
        {
            IUser oldUser = userdict.GetValueOrDefault(user.Email);
            var newUser = new User(oldUser.Email,oldUser.UserId,User.hashPassword(password));
            userdict.Remove(oldUser.Email);
            userdict.Add(newUser.Email, newUser);
            return newUser;

        }

        private IUserRoles mockGetUserPermissions(string email)
        {
            var UserPermissions = new UserRoles(email, new List<IRole>());
            UserPermissions.Roles.Add(new Role("test", "value"));
            return UserPermissions;
        }

        void MockRemoveUser(IUser user)
        {
            userdict.Remove(user.Email);
        }

        IUser MockRegister(IUserCredentials user)
        {
            // registering a user never fails!
            if (userdict.ContainsKey(user.Email))
            {
                throw new ArgumentException();
            }
            else
            {
                userdict.Add(user.Email, User.CredentialsToUser(user));
                return User.CredentialsToUser(user);
            }
        }

        IEnumerable<string> GetUserPermissions(IUser user)
        {
            return null;
        }


        List<IUser> MockGetUser()
        {
            return userdict.Values.ToList();
        }

        IUser MockGetUserByMail(string mail)
        {

            IUser user;
            userdict.TryGetValue(mail, out user);
            if (user != null)
            {
                return user;
            }
            throw new ArgumentException();
        }


        [Test]

        public void Test_GetUserRoles_For_Valid_Email_Expect_IUserRoles()
        {
            IUser user = _authService.getUsers().RandomElement();
            var roles = _rolesService.GetUserRoles(user.Email);
            roles.Email.Should().Be(user.Email);
            roles.Roles.Should().BeOfType<List<IRole>>();
        }
        [Test]
        [AutoData]
        public void Test_GetUserRoles_For_Invalid_Email_Expect_Error(string email)
        {

            Action act = () => _rolesService.GetUserRoles(email);
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void Test_GetItemRoles_For_Valid_Item_Expect_ITymelineObjectRoles()
        {

        }
        [Test]
        public void Test_GetItemRoles_For_Invalid_Item_Expect_Error()
        {
            int item = TestUtil.RandomIntWithMax(100);
            Action act = () => _rolesService.GetItemRoles(tymelineList[item].Id);
        }

        [Test]
        public void Test_GetRoles_Expect_ListOfRoles()
        {
            _rolesService.GetRoles().Should().BeOfType<List<IRole>>().And.NotBeEmpty();
        }

        [Test]
        [AutoData]
        public void Test_SetUserRoles_For_Valid_Email_Expect_UserRoles_Set(List<Role> roles)
        {
            IUser user = _authService.getUsers().RandomElement();
            List<IRole> Iroles = roles.Select(s => (IRole)s).ToList();
            IUserRoles userRole = new UserRoles(user.Email, Iroles);
            _rolesService.SetUserRoles(userRole);
            _rolesService.GetUserRoles(user.Email).Roles.Should().Contain(roles);
        }

        [Test, AutoData]
        public void Test_SetUserRoles_For_Invalid_Email_Expect_Error(string FakeMail, List<Role> roles)
        {
            List<IRole> Iroles = roles.Select(s => (IRole)s).ToList();
            IUserRoles userRole = new UserRoles(FakeMail, Iroles);
            Action act = () => _rolesService.SetUserRoles(userRole);
            act.Should().Throw<ArgumentException>();
        }

        [Test, AutoData]
        public void Test_AddRole_Expect_Role_To_Be_Saved(Role role)
        {
            Action act = () => _rolesService.AddRole(role);
            act.Should().NotThrow();
            _rolesService.GetRoles().Should().Contain(role);
        }
        [Test]
        public void Test_RemoveRole_Expect_Role_To_Be_Removed()
        {
            IRole role = _rolesService.GetRoles().RandomElement();
            _rolesService.RemoveRole(role);
            _rolesService.GetRoles().Should().NotContain(role);
        }

        [Test, AutoData]
        public void Test_AddUserRole_Impl1_For_Valid_Email_Expect_Role_In_Returned_List(Role role)
        {
            IUser user = _authService.getUsers().RandomElement();
            IUserRole userRole = new UserRole(user.Email, role);
            _rolesService.AddUserRole(userRole).Roles.Should().Contain(userRole.Role);
        }

        [Test, AutoData]
        public void Test_AddUserRole_Impl2_For_Valid_Email_Expect_Role_In_Returned_List(Role role)
        {
            IUser user = _authService.getUsers().RandomElement();
            _rolesService.AddUserRole(role, user.Email).Roles.Should().Contain(role);

        }

        [Test, AutoData]
        public void Test_AddUserRole_Impl1_For_Invalid_Email_Expect_Error(string Email, Role role)
        {
            IUserRole userRole = new UserRole(Email, role);
            Action act = () => _rolesService.AddUserRole(userRole);
            act.Should().Throw<ArgumentException>();
        }

        [Test, AutoData]
        public void Test_AddUserRole_Impl2_For_Invalid_Email_Expect_Error(string Email, Role role)
        {
            Action act = () => _rolesService.AddUserRole(role, Email);
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void Test_RemoveUserRole_Impl1_For_Valid_Email_Expect_Role_Not_In_Returned_List()
        {
            IUser user = _authService.getUsers().RandomElement();
            IUserRoles userRoles = _rolesService.GetUserRoles(user.Email);
            IRole role = userRoles.Roles.RandomElement();
            IUserRole userRole = new UserRole(user.Email, role);

            _rolesService.RemoveUserRole(userRole).Roles.Should().NotContain(role);
        }
        [Test]
        public void Test_RemoveUserRole_Impl2_For_Valid_Email_Expect_Role_Not_In_Returned_List()
        {
            IUser user = _authService.getUsers().RandomElement();
            IUserRoles userRoles = _rolesService.GetUserRoles(user.Email);
            IRole role = userRoles.Roles.RandomElement();

            _rolesService.RemoveUserRole(role, user.Email).Roles.Should().NotContain(role);
        }
        [Test, AutoData]
        public void Test_RemoveUserRole_Impl1_For_Invalid_Email_Expect_Error(string Email, Role role)
        {
            IUser user = _authService.getUsers().RandomElement();
            IUserRole userRole = new UserRole(Email, role);
            Action act = () => _rolesService.RemoveUserRole(userRole);
            act.Should().Throw<ArgumentException>();
        }
        [Test, AutoData]


        public void Test_RemoveUserRole_Impl2_For_Invalid_Email_Expect_Error(string Email, Role role)
        {
            IUser user = _authService.getUsers().RandomElement();
            Action act = () => _rolesService.RemoveUserRole(role, Email);
            act.Should().Throw<ArgumentException>();
        }




        [Test]
        public void TestGetUserRoles_For_Valid_Email_Expect_ValidIUserRoles()
        {
            IUser user = _authService.getUsers().RandomElement();
            _rolesService.GetUserRoles(user.Email).Roles.Should().BeOfType<List<IRole>>();

        }

        [Test, AutoData]
        public void TestGetUserRoles_For_Invalid_Email_expect_Exception(string Email)
        {
            Action act = () => _rolesService.GetUserRoles(Email);
            act.Should().Throw<ArgumentException>();
        }


        [Test, AutoData]
        public void Test_SetUserRoles_For_Valid_Email_expect_Result_To_Contain_Set_List(List<Role> roles)
        {
            IUser user = _authService.getUsers().RandomElement();
            UserRoles userRoles = new UserRoles(user.Email, roles.Select(s => (IRole)s).ToList());
            _rolesService.SetUserRoles(userRoles);
            _rolesService.GetUserRoles(userRoles.Email).Roles.Should().Contain(userRoles.Roles);
        }

        [Test]
        public void Test_SetUserRoles_For_Invalid_Email() { }


        [Test]
        public void Test_AddRole_New_Role() { }



    }
}