

public class UserPermission: IUserRole{

    public UserPermission(string mail, IRole perm){
        Email = mail;
        Roles = perm;
    }
    public string Email{get;set;}

    public IRole Roles{get;set;}

}



public class HttpUserPermission{

    public HttpUserPermission(string email, Role permission){
        this.Email = email;
        this.Permission = permission;
    }


    public IUserRole ToIUserRole(){
        var s = new UserPermission(this.Email, this.Permission);
        return s;
    }
    public string Email{get;set;}
    public Role Permission{get;set;} 
}