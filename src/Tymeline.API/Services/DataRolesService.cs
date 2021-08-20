using System.Collections.Generic;

public class DataRolesService : IDataRolesService
{
    public void AddRole(IRole role)
    {
        throw new System.NotImplementedException();
    }


    public List<IRole> AddRoleToItem(IRole role, string toId)
    {
        throw new System.NotImplementedException();
    }




    public List<IRole> AddUserRole(string email, IRole permission)
    {
        throw new System.NotImplementedException();
    }

    public List<IRole> AddUserRole(IUserRole userRole)
    {
        throw new System.NotImplementedException();
    }

    public ITymelineObjectRoles GetItemRoles(string toId)
    {
        throw new System.NotImplementedException();
    }

    public List<IRole> GetRoles()
    {
        throw new System.NotImplementedException();
    }

    public IUserRoles GetUserRoles(string email)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveRole(IRole role)
    {
        throw new System.NotImplementedException();
    }

    public List<IRole> RemoveRoleFromItem(IRole role, string toId)
    {
        throw new System.NotImplementedException();
    }

    public List<IRole> RemoveUserRole(string email, IRole role)
    {
        throw new System.NotImplementedException();
    }

    public List<IRole> RemoveUserRole(IUserRole userRole)
    {
        throw new System.NotImplementedException();
    }

    public void SetUserRoles(IUserRoles permissions)
    {
        throw new System.NotImplementedException();
    }
}