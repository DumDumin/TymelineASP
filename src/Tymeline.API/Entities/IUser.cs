using System;
using System.IO;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

public interface IUser{
    int UserId {
        get;
        set;
    }
    int LoggedInAt {
        get;
        set;
    }
    JwtSecurityToken createJwt(SigningCredentials credentials);
}