using System.Collections.Generic;

public class HttpUserRoles{


    public HttpUserRoles(string email, List<Role> roles)
    {
        this.Email = email;
        this.Roles = roles;
    }

    public IUserRoles toIUserRoles(){
        
        var iRoleList = new List<IRole>();
        Roles.ForEach(item => iRoleList.Add(item));
        return new UserRoles(Email, iRoleList);

    } 

    public string Email{get;set;}
    public List<Role> Roles { get; set; }
}