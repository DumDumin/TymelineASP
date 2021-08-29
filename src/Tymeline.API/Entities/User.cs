using System;



public class User : IUser
{


    
    public User(){
    }

    public User( string mail, string userId, string passwd ){
        passwordHasher = new PasswordHasher();
        Email = mail;
        UserId = userId; 
        passwordHash =  passwd;

    }

    public User( string mail, string passwd ){
        passwordHasher = new PasswordHasher();
        Email = mail;
        UserId =  Tymeline.API.HelperClass.ComputeSha256Hash(Email).Substring(0,32); 
        // this cannot function as key in database, as this is reseeded every time the application starts
        // much better would be to get the key if a user is created from the DB 
       
        passwordHash = passwd;

    }

    public static string hashPassword(string password){
        return new PasswordHasher().Hash(password);
    }

    public static User CredentialsToUser(IUserCredentials credentials){
        IPasswordHasher passwordHasher = new PasswordHasher();
        var password = passwordHasher.Hash(credentials.Password);
        return new User( credentials.Email, password);
    }


    public static User DaoUserToUser(DaoUser daoUser){
        return new User(daoUser.email,daoUser.user_id,daoUser.password);
    }

    public DaoUser ToDaoUser(){
        return new DaoUser(){email=Email,password=passwordHash,user_id=UserId};
    }

    private PasswordHasher passwordHasher {get;}
    public string UserId { get ; set; }
    private string passwordHash {get;}
    public string Email {get; set;}
    public IUser verifyPassword(string passwd){
        var (verified, needsUpgrade) = passwordHasher.Check(passwordHash, passwd);
        if (verified){
            return this;
        }
        throw new ArgumentException();
    }

    // public IUser updatePassword(string password){
    //     var passwordHash = passwordHasher.Hash(password);
    //     return new User( Email,UserId, passwordHash);

    // }
    
}




