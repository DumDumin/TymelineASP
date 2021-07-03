



using System.Collections.Generic;

public interface IUserPermissions{
    string Email { get; set;}
    List<IPermission> Permissions {get; set;}
}