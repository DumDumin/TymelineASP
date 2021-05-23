public class UserCredentials:IUserCredentials
{

    public UserCredentials(string email, string password){
        Email = email;
        Password = password;
    }
    public string Email{
        get;
        set;
    }
    public string Password {
        get;
        set;
    }
    public bool complete(){
        return !Email.Equals(null) &&  !Password.Equals(null);
    }


    
} 
