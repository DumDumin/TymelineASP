using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using Tymeline.API.Controllers;

namespace Tymeline.API.Tests
{
    public class RolesServiceTest : OneTimeSetUpAttribute
    {
        IAuthService _authService;
        private Mock<IDataRolesService> _rolesService;
        Moq.Mock<IAuthDao> _authDao;
        Dictionary<string,IUser> userdict;

        UtilService _utilService;
        IJwtService _jwtService;
        AppSettings _appSettings;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var _appSettingsOptions = Options.Create<AppSettings>(new AppSettings());
            _appSettings = _appSettingsOptions.Value;
            _authDao = new Moq.Mock<IAuthDao>();
            _utilService = new UtilService();
            _rolesService = new Mock<IDataRolesService>();
            _authService = new AuthService(_authDao.Object, _utilService,_rolesService.Object, _appSettingsOptions);
            _jwtService = new JwtService(_rolesService.Object,_appSettingsOptions);
            _rolesService.Setup(s => s.GetUserRoles(It.IsAny<string>())).Returns((string email)=> mockGetUserPermissions(email));
            _authDao.Setup(s => s.getUserByMail(It.IsAny<string>())).Returns((string mail) => MockGetUserByMail(mail));
            _authDao.Setup(s => s.GetUsers()).Returns(() => MockGetUser());
            _authDao.Setup(s => s.Register(It.IsAny<IUserCredentials>())).Returns((IUserCredentials user) => MockRegister(user));
            _authDao.Setup(s => s.GetUserPermissions(It.IsAny<IUser>())).Returns((IUser user) => GetUserPermissions(user));
            _authDao.Setup(s => s.RemoveUser(It.IsAny<IUser>())).Callback((IUser user) => MockRemoveUser(user));
            _authDao.Setup(s => s.ChangePassword(It.IsAny<IUser>(),It.IsAny<string>())).Returns((IUser user, string password) => MockChangePassword(user,password));
        }
        

        [SetUp]
        public void Setup(){
         userdict = createUserDict();
        }

        private Dictionary<string,IUser> createUserDict()
        {   
            var passwordHasher = new PasswordHasher();
            Dictionary<string,IUser> users = new Dictionary<string, IUser>();
            for (int i = 2; i < 100; i++)
            {
                User user = new User($"test{i}@email.de",passwordHasher.Hash("hunter12"));
                users.Add(user.Email,user);
            }
            return users;
        }

        IUser MockChangePassword(IUser user, string password){
            IUser oldUser = userdict.GetValueOrDefault(user.Email);
            var newUser = oldUser.updatePassword(password);
            userdict.Remove(oldUser.Email);
            userdict.Add(newUser.Email,newUser);
            return newUser;

        }

         private IUserRoles mockGetUserPermissions(string email){
            var UserPermissions = new UserPermissions(email, new List<IRole>());
            UserPermissions.Permissions.Add(new Role("test","value"));
            return UserPermissions;
        }

        void MockRemoveUser(IUser user){
            userdict.Remove(user.Email);
        }

        IUser MockRegister(IUserCredentials user){
            // registering a user never fails!
            if (userdict.ContainsKey(user.Email)){
                throw new ArgumentException();
            }
            else{
                userdict.Add(user.Email, User.CredentialsToUser(user));
                return User.CredentialsToUser(user);
            }
        }

        IEnumerable<string> GetUserPermissions(IUser user){
            return null;
        }


        Dictionary<string, IUser> MockGetUser(){
            return userdict;
        }

        IUser MockGetUserByMail(string mail){
            
            IUser user;
            userdict.TryGetValue(mail,out user);
            if (user != null){
                return user;
            }
            throw new ArgumentException();
        }

      
    }
}