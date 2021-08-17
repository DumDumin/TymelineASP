

public class UserPermission: IUserRole{

    public UserPermission(string mail, IRole perm){
        Email = mail;
        Roles = perm;
    }
    public string Email{get;set;}

    public IRole Roles{get;set;}

}
