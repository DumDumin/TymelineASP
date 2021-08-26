

public class UserRole: IUserRole{

    public UserRole(string mail, IRole perm){
        Email = mail;
        Role = perm;
    }
    public string Email{get;set;}

    public IRole Role{get;set;}

}
