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
    public AuthService(IAuthDao authDao,UtilService utilService, IOptions<AppSettings> appSettings){
        AuthDao = authDao;
        _appSettings = appSettings.Value;
        _utilService = utilService;
    }

    public SigningCredentials GetSigningCredentials()
    {
        throw new System.NotImplementedException();
    }

    public List<IUser> getUsers()
    {
        throw new System.NotImplementedException();
    }


    public IUser Login(IUserCredentials credentials)
    {
        try
        {
            if (credentials.complete())
            {
                IUser user = AuthDao.getUserById(credentials.Email.GetHashCode()) ?? throw new ArgumentNullException();
                if (user.verifyPassword(credentials.Password)){
                    return user;
                }        
            }
            throw new ArgumentException();
          }
        catch (System.Exception)
        {
            throw new ArgumentException();
        }
        
    }

    public IUser Register(IUserCredentials credentials)
    {
        if (credentials.complete() && _utilService.IsValidEmail(credentials.Email).Equals(true))
        {
            IUser user = User.CredentialsToUser(credentials);
            if (!AuthDao.GetUsers().ContainsKey(user.UserId))
            {
                AuthDao.Register(user);
                return user;
            }
        }
        throw new ArgumentException();
    }


    public IUser GetById(int id)
    {
        return AuthDao.getUserById(id);
    }

    public void RemoveUser(IUser user)
    {
        AuthDao.RemoveUser(user);
    }

    public IUser ChangePassword(IUser user, string passwd)
    {
       return AuthDao.ChangePassword(user,passwd);
    }


}
