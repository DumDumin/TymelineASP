using System.Collections.Generic;

public interface IDataRolesService
{

    // this interface deals with "Roles" - Roles are defined by a single key "blue" or "yellow"
    // users can be assigned to more than one role
    // roles can be assigned to more than one user
    // these roles just deal with 
    IUserRoles GetUserRoles(string email);
    void SetUserRoles(IUserRoles roles);
    void AddRole(string roleName);
    void RemoveRole(string roleName);
    void AddUserRole(IUserRole userRole);
    void AddUserRole(string email, IRole role);
    void RemoveUserRole(string email, IRole key);
    void RemoveUserRole(IUserRole userRole);
    // add methods for mapping roles to items

    List<IRole> AddRoleToItem(IRole role, TymelineObject to);
    void AddRoleToItems(IRole role, IEnumerable<TymelineObject> tos);
    // does this make sense?
    List<IRole> RemoveRoleFromItem(IRole role, TymelineObject to);
    void RemoveRoleFromItems(IRole role, IEnumerable<TymelineObject> tos);
    // does this make sense?

    void getItemRoles(TymelineObject to);
}
