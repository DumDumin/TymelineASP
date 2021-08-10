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

    public void AddUserPermission(string email, IPermission permission)
    {
        throw new System.NotImplementedException();
    }

    public IUserPermissions GetUserPermissions(string email)
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

    public void RemoveUserPermissions(string email, string key)
    {
        throw new System.NotImplementedException();
    }

    public void SetUserPermissions(IUserPermissions permissions)
    {
        throw new System.NotImplementedException();
    }
}