using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;

public class DataRolesDao : IDataRolesDao
{
    private MySqlConnection sqlConnection { get; set; }
    private IAuthDao _authDao;
    private ITymelineObjectDao _tymelineObjectDao;


    public DataRolesDao(MySqlConnection connection, IAuthDao authDao,ITymelineObjectDao tymelineObjectDao){
        _authDao = authDao;
        _tymelineObjectDao = tymelineObjectDao;
        sqlConnection = connection;
    }


    public void AddRole(IRole role)
    {
        sqlConnection.Open();
        MySqlTransaction transaction= sqlConnection.BeginTransaction();
        try
        {
            MySqlCommand command = new MySqlCommand();
            command.Transaction = transaction;
            command.Connection = sqlConnection;
            command.CommandText = "INSERT INTO Roles(role_id,role_name,role_value) values(@role_id,@role_name,@role_value)";
            command.Parameters.AddWithValue("@role_id",role.RoleId);
            command.Parameters.AddWithValue("@role_name",role.Type);
            command.Parameters.AddWithValue("@role_value",role.Value);
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (System.Exception)
        {
            transaction.Rollback();
            throw new ArgumentException();
        }
        finally{
            sqlConnection.Close();
        }
    }

    public ITymelineObjectRoles AddRoleToItem(IRole role, string toId)
    {
        
        sqlConnection.Open();
        MySqlTransaction transaction= sqlConnection.BeginTransaction();
        try
        {
            MySqlCommand command = new MySqlCommand();
            command.Transaction = transaction;
            command.Connection = sqlConnection;
            command.CommandText = "INSERT INTO ItemRoleRelation(item_fk,role_fk) values(@item_id,@role_id)";
            command.Parameters.AddWithValue("@item_id",toId);
            command.Parameters.AddWithValue("@role_id",role.RoleId);
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (System.Exception)
        {
            transaction.Rollback();
            throw new ArgumentException();
        }
        finally{
            sqlConnection.Close();
        }

        return GetItemRoles(toId);

        
    }

    public IUserRoles AddUserRole(IRole role, string email)
    {

        IUser user = _authDao.getUserByMail(email);
        sqlConnection.Open();
        MySqlTransaction transaction= sqlConnection.BeginTransaction();
        try
        {
            MySqlCommand command = new MySqlCommand();
            command.Transaction = transaction;
            command.Connection = sqlConnection;
            command.CommandText = "INSERT INTO UserRoleRelation(user_fk,role_fk) values(@user_id,@role_id)";
            command.Parameters.AddWithValue("@user_id",user.UserId);
            command.Parameters.AddWithValue("@role_id",role.RoleId);
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (System.Exception)
        {
            transaction.Rollback();
            throw new ArgumentException();
        }
        finally{
            sqlConnection.Close();
        }

        return GetUserRoles(email);
    }

    public List<IRole> GetAllRoles()
    {
        List<IRole> roleList;
        sqlConnection.Open();
        try
        {

            
            var sqlCmd = "Select  role_id,role_name,role_value from Roles";
            MySqlDataAdapter adr = new MySqlDataAdapter(sqlCmd, sqlConnection);
            adr.SelectCommand.CommandType = CommandType.Text;
            DataTable data = new DataTable();
            adr.Fill(data);
            roleList = data.AsEnumerable().Select(s => new Role(){
                RoleId=s.Field<int>("role_id"),
                Type=s.Field<string>("role_name"),
                Value=s.Field<string>("role_value")
                }).ToList<IRole>();
            return roleList;
            }
        catch (System.Exception)
        {
            throw;
        }
        finally{
            sqlConnection.Close();
        }
    }

    public ITymelineObjectRoles GetItemRoles(string toId)
    {
        sqlConnection.Open();
        MySqlTransaction transaction= sqlConnection.BeginTransaction();
        try
        {
            List<IRole> roleList;
            var sqlCmd = @"SELECT Roles.role_id,Roles.role_name,Roles.role_value 
            FROM Roles 
            JOIN ItemRoleRelation ir ON Roles.role_id=ir.role_fk  
            WHERE ir.item_fk=@item_id";
            MySqlDataAdapter adr = new MySqlDataAdapter(sqlCmd, sqlConnection);
            
            adr.SelectCommand.Parameters.AddWithValue("@item_id",toId);
            adr.SelectCommand.CommandType = CommandType.Text;
            DataTable data = new DataTable();
            adr.Fill(data);
            roleList = data.AsEnumerable().Select(s => new Role(){
                RoleId=s.Field<int>("role_id"),
                Type=s.Field<string>("role_name"),
                Value=s.Field<string>("role_value")
                }).ToList<IRole>();
            ITymelineObjectRoles roleObject = new TymelineObjectRoles(toId,roleList);
            return roleObject;
        }
        catch (System.Exception)
        {
            transaction.Rollback();
            throw;
        }
        finally{
            sqlConnection.Close();
        }
    }

    public IUserRoles GetUserRoles(string email)
    {
        _authDao.getUserByMail(email);
        sqlConnection.Open();
        MySqlTransaction transaction= sqlConnection.BeginTransaction();
        try
        {
            List<IRole> roleList;
            var sqlCmd = @"SELECT Roles.role_id,Roles.role_name,Roles.role_value 
            FROM Roles 
            JOIN UserRoleRelation ur ON Roles.role_id=ur.role_fk 
            JOIN Users u ON ur.user_fk=u.user_id 
            WHERE u.email=@user_email";
            MySqlDataAdapter adr = new MySqlDataAdapter(sqlCmd, sqlConnection);
            
            adr.SelectCommand.Parameters.AddWithValue("@user_email",email);
            adr.SelectCommand.CommandType = CommandType.Text;
            DataTable data = new DataTable();
            adr.Fill(data);
            roleList = data.AsEnumerable().Select(s => new Role(){
                RoleId=s.Field<int>("role_id"),
                Type=s.Field<string>("role_name"),
                Value=s.Field<string>("role_value")
                }).ToList<IRole>();
            
            IUserRoles roleObject = new UserRoles(email,roleList);
            return roleObject;
        }
        catch (System.Exception)
        {
            transaction.Rollback();
            throw;
        }
        finally{
            sqlConnection.Close();
        }
        
    }

    public void RemoveRole(IRole role)
    {
        sqlConnection.Open();
        MySqlTransaction transaction= sqlConnection.BeginTransaction();
        try
        {
            MySqlCommand command = new MySqlCommand();
            command.Transaction = transaction;
            command.Connection = sqlConnection;
            command.CommandText = "DELETE FROM Roles where  role_id=@role_id";
            command.Parameters.AddWithValue("@role_id",role.RoleId);
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (System.Exception)
        {
            transaction.Rollback();
            throw;
        }
        finally{
            sqlConnection.Close();
        }
    }

    public ITymelineObjectRoles RemoveRoleFromItem(IRole role, string toId)
    {
        _tymelineObjectDao.getById(toId);
        sqlConnection.Open();
        MySqlTransaction transaction= sqlConnection.BeginTransaction();
        try
        {
            MySqlCommand command = new MySqlCommand();
            command.Transaction = transaction;
            command.Connection = sqlConnection;
            command.CommandText = "DELETE FROM ItemRoleRelation where item_fk=@item_id and role_fk=@role_id";
            command.Parameters.AddWithValue("@item_id",toId);
            command.Parameters.AddWithValue("@role_id",role.RoleId);
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (System.Exception)
        {
            transaction.Rollback();
            throw;
        }
        finally{
            sqlConnection.Close();
        }

        return GetItemRoles(toId);
    }

    public IUserRoles RemoveUserRole(IRole role, string email)
    {
        IUser user = _authDao.getUserByMail(email);
        
        sqlConnection.Open();
        MySqlTransaction transaction= sqlConnection.BeginTransaction();
        try
        {
            MySqlCommand command = new MySqlCommand();
            command.Transaction = transaction;
            command.Connection = sqlConnection;
            command.CommandText = "DELETE FROM UserRoleRelation where user_fk=@user_id and role_fk=@role_id";
            command.Parameters.AddWithValue("@user_id",user.UserId);
            command.Parameters.AddWithValue("@role_id",role.RoleId);
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (System.Exception)
        {
            transaction.Rollback();
            throw;
        }
        finally{
            sqlConnection.Close();
        }

        return GetUserRoles(email);
    }

    public void SetUserRoles(IUserRoles roles)
    {
        throw new System.NotImplementedException();
    }
}