using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;


public class User : IUser
{
    
    public User(){
    }
    public User( string mail, int createdAt, string passwd ){
        passwordHasher = new PasswordHasher();
        Mail = mail;
        CreatedAt = createdAt;
        UserId = Mail.GetHashCode();
        passwordHash = passwd;

    }

    public static User toUser(UserRegisterCredentials credentials){
        IPasswordHasher passwordHasher = new PasswordHasher();
        var password = passwordHasher.Hash(credentials.Password);
        return new User( credentials.Email,  credentials.CreatedAt, password);
    }

    private PasswordHasher passwordHasher {get;}
    public int UserId { get ; set; }
    private string passwordHash {get;}
    public string Mail {get; set;}
    public int CreatedAt { get ; set; }

    public bool verifyPassword(string passwd){
        var (verified, needsUpgrade) = passwordHasher.Check(passwordHash, passwd);
        return verified;
    }
    
}
