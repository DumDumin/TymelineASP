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
    private readonly AppSettings _appSettings;
    public AuthService(IAuthDao authDao,IOptions<AppSettings> appSettings){
        AuthDao = authDao;
        _appSettings = appSettings.Value;
    }

    public SigningCredentials GetSigningCredentials()
    {
        throw new System.NotImplementedException();
    }

    public List<IUser> getUsers()
    {
        throw new System.NotImplementedException();
    }

    public IUser Login(UserCredentials credentials)
    {
        throw new System.NotImplementedException();
    }

    public IUser Register(UserRegisterCredentials credentials)
    {
        throw new System.NotImplementedException();
    }

    public string createPassword(User BaseUser)
    {
        throw new System.NotImplementedException();
    }

    public bool verifyPassword(string Password, User BaseUser)
    {
        throw new System.NotImplementedException();
    }

    public string CreateJWT(IUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("id", user.UserId.ToString()) }),
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
        throw new NotImplementedException();
    }
}
