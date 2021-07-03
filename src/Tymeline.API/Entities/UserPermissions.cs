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