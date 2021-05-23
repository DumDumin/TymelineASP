using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;


public class User : IUser
{
    
    public User(){
    }
    public User( string mail, string passwd ){
        passwordHasher = new PasswordHasher();
        Mail = mail;
        UserId = Mail.GetHashCode();
        passwordHash = passwd;

    }

    public static User CredentialsToUser(IUserCredentials credentials){
        IPasswordHasher passwordHasher = new PasswordHasher();
        var password = passwordHasher.Hash(credentials.Password);
        return new User( credentials.Email, password);
    }

    private PasswordHasher passwordHasher {get;}
    public int UserId { get ; set; }
    private string passwordHash {get;}
    public string Mail {get; set;}
    public bool verifyPassword(string passwd){
        var (verified, needsUpgrade) = passwordHasher.Check(passwordHash, passwd);
        return verified;
    }

    public IUser updatePassword(string password){
        var passwordHash = passwordHasher.Hash(password);
        return new User( Mail, passwordHash);

    }
    
}
