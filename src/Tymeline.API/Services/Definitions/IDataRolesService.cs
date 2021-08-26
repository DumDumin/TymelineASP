using System.Collections.Generic;

public interface IDataRolesService
{

    // this interface deals with "Roles" - Roles are defined by a single key "blue" or "yellow"
    // users can be assigned to more than one role
    // roles can be assigned to more than one user
    // these roles just deal with 
    IUserRoles GetUserRoles(string email);
    ITymelineObjectRoles GetItemRoles(string toId);
    List<IRole> GetRoles();
    void SetUserRoles(IUserRoles roles);

    void AddRole(IRole role);
    
    void RemoveRole(IRole role);
    List<IRole> AddUserRole(IUserRole userRole);
    List<IRole> AddUserRole(IRole role,string email);
    List<IRole> RemoveUserRole(IRole role,string email);
     List<IRole> RemoveUserRole(IUserRole userRole);
    // add methods for mapping roles to items

    List<IRole> AddRoleToItem(IRole role, string toId);
    // void AddRoleToItems(IRole role, IEnumerable<TymelineObject> tos);
    // does this make sense?
    List<IRole> RemoveRoleFromItem(IRole role, string toId);
    // void RemoveRoleFromItems(IRole role, IEnumerable<TymelineObject> tos);
    // does this make sense?

    
}
