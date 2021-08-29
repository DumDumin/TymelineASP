using System.Collections.Generic;
using MySql.Data.MySqlClient;

public class DataRolesDao : IDataRolesDao
{
    public MySqlConnection sqlConnection { get; private set; }


    public DataRolesDao(MySqlConnection connection){
        sqlConnection = connection;
    }


    public void AddRole(IRole role)
    {
        throw new System.NotImplementedException();
    }

    public List<IRole> AddRoleToItem(IRole role, string toId)
    {
        throw new System.NotImplementedException();
    }

    public List<IRole> AddUserRole(IRole role, string email)
    {
        throw new System.NotImplementedException();
    }

    public List<IRole> GetAllRoles()
    {
        throw new System.NotImplementedException();
    }

    public ITymelineObjectRoles GetItemRoles(string toId)
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

    public List<IRole> RemoveUserRole(IRole role, string email)
    {
        throw new System.NotImplementedException();
    }

    public void SetUserRoles(IUserRoles roles)
    {
        throw new System.NotImplementedException();
    }
}