using System.Collections.Generic;

public class DataRolesService : IDataRolesService
{
    public IDataRolesDao _rolesDao { get; private set; }



    public DataRolesService(IDataRolesDao rolesDao){
        _rolesDao = rolesDao;
    }


    public void AddRole(IRole role)
    {
        _rolesDao.AddRole(role);
    }


    public List<IRole> AddRoleToItem(IRole role, string toId)
    {
        return _rolesDao.AddRoleToItem(role,toId);
    }




    public List<IRole> AddUserRole(IRole role,string email)
    {
        return _rolesDao.AddUserRole(role,email);
        
    }

    public List<IRole> AddUserRole(IUserRole userRole)
    {
        return _rolesDao.AddUserRole(userRole.Role,userRole.Email);
    }

    public ITymelineObjectRoles GetItemRoles(string toId)
    {
        return _rolesDao.GetItemRoles(toId);
    }

    public List<IRole> GetRoles()
    {
        return _rolesDao.GetAllRoles();
    }

    public IUserRoles GetUserRoles(string email)
    {
        return _rolesDao.GetUserRoles(email);
    }

    public void RemoveRole(IRole role)
    {
        _rolesDao.RemoveRole(role);
    }

    public List<IRole> RemoveRoleFromItem(IRole role, string toId)
    {
        return _rolesDao.RemoveRoleFromItem(role,toId);
    }

    public List<IRole> RemoveUserRole(IRole role,string email)
    {
        return _rolesDao.RemoveUserRole(role,email);
    }

    public List<IRole> RemoveUserRole(IUserRole userRole)
    {
        return _rolesDao.RemoveUserRole(userRole.Role,userRole.Email);
    }

    public void SetUserRoles(IUserRoles permissions)
    {
        _rolesDao.SetUserRoles(permissions);
    }
}