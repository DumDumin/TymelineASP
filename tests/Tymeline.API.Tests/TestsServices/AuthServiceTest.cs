using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;


namespace Tymeline.API.Tests
{   
    [Category("Service")]
    public class AuthServiceTest : OneTimeSetUpAttribute
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
            
            userdict = createUserDict();
            
            _authDao = new Moq.Mock<IAuthDao>();
            _utilService = new UtilService();
            _rolesService = new Mock<IDataRolesService>();
            _authService = new AuthService(_authDao.Object, _utilService,_rolesService.Object, _appSettingsOptions);
            _jwtService = new JwtService(_authService,_appSettingsOptions);
            _rolesService.Setup(s => s.GetUserRoles(It.IsAny<string>())).Returns((string email)=> mockGetUserPermissions(email));
            _authDao.Setup(s => s.getUserByMail(It.IsAny<string>())).Returns((string mail) => MockGetUserByMail(mail));
            _authDao.Setup(s => s.GetUsers()).Returns(() => MockGetUser());
            _authDao.Setup(s => s.Register(It.IsAny<IUserCredentials>())).Returns((IUserCredentials user) => MockRegister(user));
            _authDao.Setup(s => s.RemoveUser(It.IsAny<IUser>())).Callback((IUser user) => MockRemoveUser(user));
            _authDao.Setup(s => s.ChangePassword(It.IsAny<IUser>(),It.IsAny<string>())).Returns((IUser user, string password) => MockChangePassword(user,password));
        }
        

        [SetUp]
        public void Setup(){
         
        }

        IUser MockChangePassword(IUser user, string password){
            IUser oldUser = userdict.GetValueOrDefault(user.Email);
            
            var newUser = new User(oldUser.Email,oldUser.UserId,User.hashPassword(password));
            userdict.Remove(oldUser.Email);
            userdict.Add(newUser.Email,newUser);
            return newUser;

        }

         private IUserRoles mockGetUserPermissions(string email){
            var UserPermissions = new UserRoles(email, new List<IRole>());
            UserPermissions.Roles.Add(new Role("test","value"));
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
                var credentialedUser=User.CredentialsToUser(user);
                userdict.Add(user.Email, credentialedUser);
                return credentialedUser;
            }
        }

        IEnumerable<string> GetUserPermissions(IUser user){
            return null;
        }


        List<IUser> MockGetUser(){
            return userdict.Values.ToList();
        }

        IUser MockGetUserByMail(string mail){
            
            IUser user;
            if(userdict.TryGetValue(mail,out user)){
                return user;
            }
            throw new ArgumentException();
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
        [Test]
        public void Test_Login_Given_Valid_Credentials_Expect_IUser(){

            string mail = "test15@email.de";
            IUserCredentials credentials = new UserCredentials(mail,"hunter12");
            IUser user = _authService.Login(credentials);
            Assert.NotNull(user);
            Assert.AreEqual(user.Email,mail);
        }
        [Test]
        public void Test_Login_Given_Invalid_Credentials_Expect_ArgumentException(){
            string mail = "test15123@email.de";
            
            IUserCredentials credentials = new UserCredentials(mail,"hunter12");
            Assert.Throws<ArgumentException>(() => _authService.Login(credentials));
        }

        [Test]
        public void Test_Login_Given_Invalid_Password_Expect_ArgumentException(){
            string mail = "test5@email.de";
            IUserCredentials credentials = new UserCredentials(mail,"hunter123");
            Assert.Throws<ArgumentException>(() => _authService.Login(credentials));
        }

    
        [Test,AutoData]
        public void Test_Register_Given_Valid_Credentials_Expect_IUser(string emailPrefix){
            string mail = $"{emailPrefix}@email.de";
            IUserCredentials credentials = new UserCredentials(mail,"hunter13");
            IUser user =_authService.Register(credentials);
            Assert.NotNull(user);
            Assert.AreEqual(user.Email,mail);
        }



        [Test,AutoData]
        public void Test_Register_Given_Valid_Credentials_Expect_Login_To_Succeed(string password){
            string mail= "thisisatestemail@email.de";
            IUserCredentials credentials = new UserCredentials(mail,password);
            _authService.Register(credentials);
            IUser user =_authService.Login(credentials);
            Assert.NotNull(user);
            Assert.AreEqual(user.Email,mail);
        }


        [Test,AutoData]
        public void Test_Register_Given_Invalid_Credentials_Expect_NullUser(string email){
            // WHAT ARE INVALID CREDENTIALS?!?!
            string mail = email;
            IUserCredentials credentials = new UserCredentials(mail,"hunter13");
            Assert.Throws<ArgumentException>(() => _authService.Register(credentials));
        }

        [Test]
        public void Test_Register_Given_Already_Registered_Credentials_Expect_ArgumentException(){
            var randomElement = _authService.getUsers().RandomElement();
            IUserCredentials credentials = new UserCredentials(randomElement.Email,"hunter13");
            
            Assert.Throws<ArgumentException>(() => _authService.Register(credentials));
            
        }


        [Test]
        public void Test_Remove_Account_Expect_Account_To_Throw_Error_On_Retrival(){
            string mail = "test5@email.de";
            string passwd ="hunter13";
            IUserCredentials creds = new UserCredentials(mail,passwd);
            var user = User.CredentialsToUser(creds);
            _authService.RemoveUser(user);
            Assert.Throws<ArgumentException>(() => _authService.GetByMail(user.Email));
        }


        [Test]
        public void Test_Remove_Account__Expect_No_Error_From_Removing(){
            string mail = "test5@email.de";
            string passwd ="hunter13";
            IUserCredentials creds = new UserCredentials(mail,passwd);
            var user = User.CredentialsToUser(creds);
            Action act = () =>_authService.RemoveUser(user);
            Action act2 = () =>_authService.RemoveUser(user);
            act.Should().NotThrow();
            act2.Should().NotThrow();
            Action getByMail = () => _authService.GetByMail(user.Email);
            getByMail.Should().Throw<ArgumentException>();
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
            Assert.AreEqual(loginUser.Email,mail);
        }


          [Test,AutoData]
        public void Test_Change_Password_Expect_UserPasswordChangeIdempotency(string mailPrefix){
            // the old User is still valid after it has been changed.
            // important for groups. Users shoudlnt be affected negativly by this behaviour
            
            string mail = $"{mailPrefix}@email.de";
            string passwd = "hunter13";
            IUserCredentials credentials = new UserCredentials(mail,passwd);
            IUser user = _authService.Register(credentials);
            _authService.ChangePassword(user,"changedPassword");
            _authService.ChangePassword(user,"changedPasswor2");
            IUserCredentials creds = new UserCredentials(mail,"changedPasswor2");
            IUser loginUser =_authService.Login(creds);
            Assert.NotNull(loginUser);
            Assert.AreEqual(loginUser.Email,mail);
        }


        [Test]
        public void Test_JWT(){
            string mail = "test105@email.de";
            string passwd = "hunter13";
            IUserCredentials credentials = new UserCredentials(mail,passwd);
            IUser user = User.CredentialsToUser(credentials);
            var jwtString = _jwtService.createJwt(user.Email);
            Assert.NotNull(jwtString);
        }
    }
}