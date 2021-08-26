using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class UserRoles : IUserRoles
{

    public UserRoles(){}

    public UserRoles(string email,List<IRole> permissions){
        this.Email = email;
        this.Roles = permissions;
    }

    public string Email {get; set;}

    [JsonConverter(typeof(ListIPermissionsConverter))]
    public List<IRole> Roles { get; set; }

}
