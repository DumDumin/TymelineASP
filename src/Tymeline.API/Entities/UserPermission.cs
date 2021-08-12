

public class UserPermission: IUserPermission{

    public UserPermission(string mail, IPermission perm){
        Email = mail;
        Permission = perm;
    }
    public string Email{get;set;}

    public IPermission Permission{get;set;}

}



public class HttpUserPermission{

    public HttpUserPermission(string email, Permission permission){
        this.Email = email;
        this.Permission = permission;
    }


    public IUserPermission ToIUserPermission(){
        var s = new UserPermission(this.Email, this.Permission);
        return s;
    }
    public string Email{get;set;}
    public Permission Permission{get;set;} 
}