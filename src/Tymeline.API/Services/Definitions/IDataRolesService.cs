using System.Collections.Generic;

public interface IDataRolesService
{

    // this interface deals with "Roles" - Roles are defined by a single key "blue" or "yellow"
    // users can be assigned to more than one role
    // roles can be assigned to more than one user
    // these roles just deal with




    bool UserHasAccessToItem(string Email, string toId);
    bool UserHasAccessToItem(string Email, string toId, Roles minPermission);
    bool UserHasAccessToItem(IUser User, string toId, Roles minPermission);
    IUserRoles GetUserRoles(string email);
    ITymelineObjectRoles GetItemRoles(string toId);
    List<IRole> GetRoles();
    void SetUserRoles(IUserRoles roles);

    void AddRole(IRole role);

    void RemoveRole(IRole role);
    IUserRoles AddUserRole(IUserRole userRole);
    IUserRoles AddUserRole(IRole role, string email);
    IUserRoles RemoveUserRole(IRole role, string email);
    IUserRoles RemoveUserRole(IUserRole userRole);
    // add methods for mapping roles to items

    ITymelineObjectRoles AddRoleToItem(IRole role, string toId);
    // void AddRoleToItems(IRole role, IEnumerable<TymelineObject> tos);
    // does this make sense?
    ITymelineObjectRoles RemoveRoleFromItem(IRole role, string toId);
    // void RemoveRoleFromItems(IRole role, IEnumerable<TymelineObject> tos);
    // does this make sense?


}
