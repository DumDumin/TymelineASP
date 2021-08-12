using System.Collections.Generic;
using Newtonsoft.Json;

public class UserPermissions : IUserRoles
{

    public UserPermissions(){

    }
    public UserPermissions(string email,List<IRole> permissions){
        this.Email = email;
        this.Permissions = permissions;
    }

    public string Email {get; set;}

    [JsonConverter(typeof(ListIPermissionsConverter))]
    public List<IRole> Permissions { get; set; }
}

public class HttpUserPermissions{


    public HttpUserPermissions(string email, List<Role> permissions)
    {
        this.Email = email;
        this.Permissions = permissions;
    }

    public IUserRoles toIUserPermissions(){
        
        var iPermissionList = new List<IRole>();
        Permissions.ForEach(item => iPermissionList.Add(item));
        return new UserPermissions(Email, iPermissionList);

    } 

    public string Email{get;set;}
    public List<Role> Permissions { get; set; }
}