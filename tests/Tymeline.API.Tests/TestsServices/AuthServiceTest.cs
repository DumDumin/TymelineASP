using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;


namespace Tymeline.API.Tests
{
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

        IUser MockChangePassword(IUser user, string password){
            IUser oldUser = userdict.GetValueOrDefault(user.Email);
            var newUser = oldUser.updatePassword(password);
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

    
        [Test]
        public void Test_Register_Given_Valid_Credentials_Expect_IUser(){
            string mail = "test105@email.de";
            IUserCredentials credentials = new UserCredentials(mail,"hunter13");
            IUser user =_authService.Register(credentials);
            Assert.NotNull(user);
            Assert.AreEqual(user.Email,mail);
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
            Assert.AreEqual(user.Email,mail);
        }


        [Test]
        public void Test_Register_Given_Invalid_Credentials_Expect_NullUser(){
            string mail = "test105email.de";
            IUserCredentials credentials = new UserCredentials(mail,"hunter13");
            Assert.Throws<ArgumentException>(() => _authService.Register(credentials));
        }

        [Test]
        public void Test_Register_Given_Already_Registered_Credentials_Expect_ArgumentException(){
            string mail = "test5@email.de";
            IUserCredentials credentials = new UserCredentials(mail,"hunter13");
            
            Assert.Throws<ArgumentException>(() => _authService.Register(credentials));
            
        }


        [Test]
        public void Test_Remove_Account_Expect_ArgumentException(){
            string mail = "test5@email.de";
            string passwd ="hunter13";
            IUserCredentials creds = new UserCredentials(mail,passwd);
            var user = User.CredentialsToUser(creds);
            _authService.RemoveUser(user);
            Assert.Throws<ArgumentException>(() => _authService.GetByMail(user.Email));
        }


        [Test]
        public void Test_Remove_Account_Twice_Expect_ArgumentException(){
            string mail = "test5@email.de";
            string passwd ="hunter13";
            IUserCredentials creds = new UserCredentials(mail,passwd);
            var user = User.CredentialsToUser(creds);
            _authService.RemoveUser(user);
            _authService.RemoveUser(user);
           Assert.Throws<ArgumentException>(() => _authService.GetByMail(user.Email));
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