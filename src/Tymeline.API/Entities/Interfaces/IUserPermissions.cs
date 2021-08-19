



using System.Collections.Generic;

public interface IUserRoles{
    string Email { get; set;}
    List<IRole> Roles {get; set;}
}