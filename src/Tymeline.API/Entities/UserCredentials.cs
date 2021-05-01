public class UserCredentials
{
public string Email{
    get;
    set;
}
 public string Password {
    get;
    set;
}
    
} 

public class UserRegisterCredentials: UserCredentials{

public int CreatedAt{
    get;
    set;
}

}