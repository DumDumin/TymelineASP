using System.Collections.Generic;

public interface IDataRolesService
{

    // this interface deals with "Roles" - Roles are defined by a single key "blue" or "yellow"
    // users can be assigned to more than one role
    // roles can be assigned to more than one user
    // these roles just deal with 
    IUserPermissions GetUserPermissions(string email);
    void SetUserPermissions(IUserPermissions permissions);
    void AddRole(string roleName);
    void RemoveRole(string roleName);
    void AddUserPermission(string email, IPermission permission);
    void RemoveUserPermissions(string email, string key);
    // add methods for mapping roles to items

    void addRoleToItem(string role, TymelineObject to);
    void AddRoleToItems(string role, IEnumerable<TymelineObject> tos);
    void RemoveRoleFromItem(string role, TymelineObject to);
    void RemoveRoleFromItems(string role, IEnumerable<TymelineObject> tos);
}
