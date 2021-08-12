using System.Collections.Generic;

public class DataRolesService : IDataRolesService
{
    public void AddRole(string roleName)
    {
        throw new System.NotImplementedException();
    }

    public void addRoleToItem(string role, TymelineObject to)
    {
        throw new System.NotImplementedException();
    }

    public void AddRoleToItems(string role, IEnumerable<TymelineObject> tos)
    {
        throw new System.NotImplementedException();
    }

    public void AddUserRole(string email, IRole permission)
    {
        throw new System.NotImplementedException();
    }

    public void AddUserRole(IUserRole userRole)
    {
        throw new System.NotImplementedException();
    }

    public IUserRoles GetUserRoles(string email)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveRole(string roleName)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveRoleFromItem(string role, TymelineObject to)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveRoleFromItems(string role, IEnumerable<TymelineObject> tos)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveUserRole(string email, IRole role)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveUserRole(IUserRole userRole)
    {
        throw new System.NotImplementedException();
    }

    public void SetUserRoles(IUserRoles permissions)
    {
        throw new System.NotImplementedException();
    }
}