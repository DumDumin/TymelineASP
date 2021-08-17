using System.Collections.Generic;
using Newtonsoft.Json;

public class UserRoles : IUserRoles
{

    public UserRoles(){

    }
    public UserRoles(string email,List<IRole> permissions){
        this.Email = email;
        this.Permissions = permissions;
    }

    public string Email {get; set;}

    [JsonConverter(typeof(ListIPermissionsConverter))]
    public List<IRole> Permissions { get; set; }
}
