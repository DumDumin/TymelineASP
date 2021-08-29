using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class AuthService : IAuthService
{   
    private IAuthDao AuthDao;
    private UtilService _utilService;
    private readonly AppSettings _appSettings;
    private IDataRolesService _dataRolesService;
    public AuthService(
    IAuthDao authDao,
    UtilService utilService,
    IDataRolesService dataRolesService,
    IOptions<AppSettings> appSettings){
        AuthDao = authDao;
        _dataRolesService = dataRolesService;
        _appSettings = appSettings.Value;
        _utilService = utilService;
    }

    public SigningCredentials GetSigningCredentials()
    {
        throw new System.NotImplementedException();
    }

    public List<IUser> getUsers()
    {
        return AuthDao.GetUsers();
    }


    public IUser Login(IUserCredentials credentials)
    {
       
        if (credentials.complete())
        {
            return AuthDao.getUserByMail(credentials.Email).verifyPassword(credentials.Password);
        }
        throw new ArgumentException();
    }

    public IUser Register(IUserCredentials credentials)
    {
        if (credentials.complete() && _utilService.IsValidEmail(credentials.Email).Equals(true))
        {            
            return AuthDao.Register(credentials);       
        }
        throw new ArgumentException();
    }


    public IUser GetByMail(string mail)
    {
        return AuthDao.getUserByMail(mail);
    }

    public void RemoveUser(IUser user)
    {
        AuthDao.RemoveUser(user);
    }

    public IUser ChangePassword(IUser user, string passwd)
    {
       return AuthDao.ChangePassword(user,passwd);
    }

    public IUserRoles GetUserRoles(string email)
    {
        return _dataRolesService.GetUserRoles(email);
    }

    public void SetUserRoles(IUserRoles userRole)
    {
        _dataRolesService.SetUserRoles(userRole);
    }

    public void AddUserRole(IUserRole userRole)
    {
        _dataRolesService.AddUserRole(userRole.Role,userRole.Email);
    }

    public void RemoveUserRole(IUserRole userRole){
        _dataRolesService.RemoveUserRole(userRole.Role,userRole.Email);

    }
    
}
