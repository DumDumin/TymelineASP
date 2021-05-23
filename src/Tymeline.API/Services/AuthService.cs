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
            return null;
          }
        catch (System.Exception)
        {
            return null;
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
        return null;
    }

    public string CreateJWT(IUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { 
                // new Claim("id", user.UserId.ToString()),
                new Claim(ClaimTypes.Actor,user.UserId.ToString())
                }),
            Audience = _appSettings.Hostname,
            Issuer = _appSettings.Hostname,
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public bool verifyJWT(string jwt, IUser user)
    {
        throw new System.NotImplementedException();
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
