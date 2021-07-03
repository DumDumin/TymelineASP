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
        UserId = Mail.GetHashCode(); // this cannot function as key in database, as this is reseeded every time the application starts
        // much better would be to get the key if a user is created from the DB
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
    public IUser verifyPassword(string passwd){
        var (verified, needsUpgrade) = passwordHasher.Check(passwordHash, passwd);
        if (verified){
            return this;
        }
        throw new ArgumentException();
    }

    public IUser updatePassword(string password){
        var passwordHash = passwordHasher.Hash(password);
        return new User( Mail, passwordHash);

    }
    
}
