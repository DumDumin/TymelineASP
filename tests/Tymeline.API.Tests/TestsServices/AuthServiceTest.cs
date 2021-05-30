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
    public class AuthServiceTest : OneTimeSetUpAttribute
    {
        IAuthService _authService;
        Moq.Mock<IAuthDao> _authDao;
        Dictionary<int,IUser> userdict;

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
            _authService = new AuthService(_authDao.Object, _utilService, _appSettingsOptions);
            _jwtService = new JwtService(_appSettingsOptions);
            _authDao.Setup(s => s.getUserById(It.IsAny<int>())).Returns((int id) => MockGetUserById(id));
            _authDao.Setup(s => s.GetUsers()).Returns(() => MockGetUser());
            _authDao.Setup(s => s.Register(It.IsAny<IUser>())).Returns((IUser user) => MockRegister(user));
            _authDao.Setup(s => s.GetUserPermissions(It.IsAny<IUser>())).Returns((IUser user) => GetUserPermissions(user));
            _authDao.Setup(s => s.RemoveUser(It.IsAny<IUser>())).Callback((IUser user) => MockRemoveUser(user));
            _authDao.Setup(s => s.ChangePassword(It.IsAny<IUser>(),It.IsAny<string>())).Returns((IUser user, string password) => MockChangePassword(user,password));
        }
        

        [SetUp]
        public void Setup(){
         userdict = createUserDict();
        }

        IUser MockChangePassword(IUser user, string password){
            IUser oldUser = userdict.GetValueOrDefault(user.UserId);
            var newUser = oldUser.updatePassword(password);
            userdict.Remove(oldUser.UserId);
            userdict.Add(newUser.UserId,newUser);
            return newUser;

        }

        void MockRemoveUser(IUser user){
            userdict.Remove(user.UserId);
        }

        IUser MockRegister(IUser user){
            // registering a user never fails!
            if (userdict.ContainsKey(user.UserId)){
                return null;
            }
            else{
                userdict.Add(user.UserId, user);
                return user;
            }
        }

        IEnumerable<string> GetUserPermissions(IUser user){
            return null;
        }


        Dictionary<int, IUser> MockGetUser(){
            return userdict;
        }

        IUser MockGetUserById(int id){
            
            return userdict.GetValueOrDefault(id,null);
        }

        JwtSecurityToken MockAuthMiddleware(string token){
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidIssuer = _appSettings.Hostname,
                ValidAudience = _appSettings.Hostname,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            return jwtToken;
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
        [Test]
        public void Test_Login_Given_Valid_Credentials_Expect_IUser(){

            string mail = "test15@email.de";
            IUserCredentials credentials = new UserCredentials(mail,"hunter12");
            IUser user = _authService.Login(credentials);
            Assert.NotNull(user);
            Assert.AreEqual(user.UserId,mail.GetHashCode());
        }
        [Test]
        public void Test_Login_Given_Invalid_Credentials_Expect_NullUser(){
            string mail = "test15123@email.de";
            
            IUserCredentials credentials = new UserCredentials(mail,"hunter12");
            IUser user = _authService.Login(credentials);
            Assert.IsNull(user);
        }

        [Test]
        public void Test_Login_Given_Invalid_Password_Expect_NullUser(){
            string mail = "test5@email.de";
            IUserCredentials credentials = new UserCredentials(mail,"hunter123");
            IUser user = _authService.Login(credentials);
            Assert.IsNull(user);
        }

    
        [Test]
        public void Test_Register_Given_Valid_Credentials_Expect_IUser(){
            string mail = "test105@email.de";
            IUserCredentials credentials = new UserCredentials(mail,"hunter13");
            IUser user =_authService.Register(credentials);
            Assert.NotNull(user);
            Assert.AreEqual(user.UserId,mail.GetHashCode());
        }



        [Test]
        public void Test_Register_Given_Valid_Credentials_Expect_Login_To_Succeed(){
            string mail = "test105@email.de";
            string passwd = "hunter13";
            IUserCredentials credentials = new UserCredentials(mail,passwd);
            IUserCredentials creds = new UserCredentials(mail,passwd);
            _authService.Register(credentials);
            IUser user =_authService.Login(creds);
            Assert.NotNull(user);
            Assert.AreEqual(user.UserId,mail.GetHashCode());
        }


        [Test]
        public void Test_Register_Given_Invalid_Credentials_Expect_NullUser(){
            string mail = "test105email.de";
            IUserCredentials credentials = new UserCredentials(mail,"hunter13");
            IUser user =_authService.Register(credentials);
            Assert.IsNull(user);
        }

        [Test]
        public void Test_Register_Given_Already_Registered_Credentials_Expect_NullUser(){
            string mail = "test5@email.de";
            IUserCredentials credentials = new UserCredentials(mail,"hunter13");
            IUser user =_authService.Register(credentials);
            Assert.IsNull(user);
        }


        [Test]
        public void Test_Remove_Account_Expect_User_To_Not_Exist_After(){
            string mail = "test5@email.de";
            string passwd ="hunter13";
            IUserCredentials creds = new UserCredentials(mail,passwd);
            var user = User.CredentialsToUser(creds);
            _authService.RemoveUser(user);
            IUser shouldBeNull = _authService.GetById(user.UserId);
            Assert.IsNull(shouldBeNull);
        }


        [Test]
        public void Test_Remove_Account_Idempotency(){
            string mail = "test5@email.de";
            string passwd ="hunter13";
            IUserCredentials creds = new UserCredentials(mail,passwd);
            var user = User.CredentialsToUser(creds);
            _authService.RemoveUser(user);
            _authService.RemoveUser(user);
            IUser shouldBeNull = _authService.GetById(user.UserId);
            Assert.IsNull(shouldBeNull);
        }

        [Test]
        public void Test_Change_Password_Expect_Login_With_New_Password_Success(){
            string mail = "test105@email.de";
            string passwd = "hunter13";
            IUserCredentials credentials = new UserCredentials(mail,passwd);
            IUser user = _authService.Register(credentials);
            _authService.ChangePassword(user,"changedPassword");
            IUserCredentials creds = new UserCredentials(mail,"changedPassword");
             IUser loginUser =_authService.Login(creds);
            Assert.NotNull(loginUser);
            Assert.AreEqual(loginUser.UserId,mail.GetHashCode());
        }


          [Test]
        public void Test_Change_Password_Expect_UserPasswordChangeIdempotency(){
            // the old User is still valid after it has been changed.
            // important for groups. Users shoudlnt be affected negativly by this behaviour
            string mail = "test105@email.de";
            string passwd = "hunter13";
            IUserCredentials credentials = new UserCredentials(mail,passwd);
            IUser user = _authService.Register(credentials);
            _authService.ChangePassword(user,"changedPassword");
            _authService.ChangePassword(user,"changedPasswor2");
            IUserCredentials creds = new UserCredentials(mail,"changedPasswor2");
            IUser loginUser =_authService.Login(creds);
            Assert.NotNull(loginUser);
            Assert.AreEqual(loginUser.UserId,mail.GetHashCode());
        }


        [Test]
        public void Test_JWT(){
            string mail = "test105@email.de";
            string passwd = "hunter13";
            IUserCredentials credentials = new UserCredentials(mail,passwd);
            IUser user = User.CredentialsToUser(credentials);
            var jwtString = _jwtService.createJwt(user);
            Assert.NotNull(jwtString);
        }
    }
}