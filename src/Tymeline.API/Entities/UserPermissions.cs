using System.Collections.Generic;
using Newtonsoft.Json;

public class UserPermissions : IUserPermissions
{

    public UserPermissions(){

    }
    public UserPermissions(string email,List<IPermission> permissions){
        this.Email = email;
        this.Permissions = permissions;
    }

    public string Email {get; set;}

    [JsonConverter(typeof(ListIPermissionsConverter))]
    public List<IPermission> Permissions { get; set; }
}

public class HttpUserPermissions{


    public HttpUserPermissions(string email, List<Permission> permissions)
    {
        this.Email = email;
        this.Permissions = permissions;
    }

    public IUserPermissions toIUserPermissions(){
        
        var iPermissionList = new List<IPermission>();
        Permissions.ForEach(item => iPermissionList.Add(item));
        return new UserPermissions(Email, iPermissionList);

    } 

    public string Email{get;set;}
    public List<Permission> Permissions { get; set; }
}