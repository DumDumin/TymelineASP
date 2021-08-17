using System.Collections.Generic;

public class HttpUserPermissions{


    public HttpUserPermissions(string email, List<Role> permissions)
    {
        this.Email = email;
        this.Permissions = permissions;
    }

    public IUserRoles toIUserRoles(){
        
        var iPermissionList = new List<IRole>();
        Permissions.ForEach(item => iPermissionList.Add(item));
        return new UserRoles(Email, iPermissionList);

    } 

    public string Email{get;set;}
    public List<Role> Permissions { get; set; }
}