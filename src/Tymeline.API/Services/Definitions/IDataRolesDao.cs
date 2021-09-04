using System.Collections.Generic;

public interface IDataRolesDao{

    void AddRole(IRole role);
    ITymelineObjectRoles AddRoleToItem(IRole role, string toId);
    IUserRoles AddUserRole(IRole role, string email);

    ITymelineObjectRoles GetItemRoles(string toId);
    List<IRole> GetAllRoles();
    IUserRoles GetUserRoles(string email);

    void RemoveRole(IRole role);

    ITymelineObjectRoles RemoveRoleFromItem(IRole role, string toId);
    IUserRoles RemoveUserRole(IRole role,string email);

    void SetUserRoles(IUserRoles roles);
}