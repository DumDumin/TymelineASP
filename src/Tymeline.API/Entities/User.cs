using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

public class User : IUser
{
    public User(){

    }
    public User( string mail, int loggedIn){
        Mail = mail;
        LoggedInAt = loggedIn;
        UserId = Mail.GetHashCode();
    }
    public int UserId { get ; set; }
    public string Mail {get; set;}
    public int LoggedInAt { get ; set; }

    public JwtSecurityToken createJwt(SigningCredentials credentials)
    {
        
        // create claims management to get available claims injected in here!
        JwtHeader header =  new JwtHeader(credentials);
        List<Claim> claims = new List<Claim>();
    
        claims.Add( new Claim(JwtRegisteredClaimNames.Sub, UserId.ToString()));
        claims.Add( new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
    
        return new JwtSecurityToken(issuer:"tymeline.de",audience:"tymeline.de",claims,signingCredentials:credentials);
    }
}