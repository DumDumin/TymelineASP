using System.Collections.Generic;

public interface IDataRolesDao{

    void AddRole(IRole role);
    List<IRole> AddRoleToItem(IRole role, string toId);
    List<IRole> AddUserRole(IRole role, string email);

    ITymelineObjectRoles GetItemRoles(string toId);
    List<IRole> GetAllRoles();
    IUserRoles GetUserRoles(string email);

    void RemoveRole(IRole role);

    List<IRole> RemoveRoleFromItem(IRole role, string toId);
    List<IRole> RemoveUserRole(IRole role,string email);

    void SetUserRoles(IUserRoles roles);
}