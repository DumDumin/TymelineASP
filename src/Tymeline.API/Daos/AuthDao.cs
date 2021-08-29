using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;

public class AuthDao : IAuthDao
{

    MySqlConnection sqlConnection;
    public AuthDao(MySqlConnection connection)
        {
            sqlConnection = connection;
        }


    public IUser ChangePassword(IUser user, string password)
    {
        sqlConnection.Open();
        password = User.hashPassword(password);
            MySqlTransaction trans = sqlConnection.BeginTransaction();
            try
            {

                MySqlCommand command = new MySqlCommand();
                command.Transaction = trans;
                command.Connection = sqlConnection;
                command.CommandText = @"Update Users
                SET password=@password
                where user_id=@id
                ";
                command.Parameters.AddWithValue("@id", user.UserId);
                command.Parameters.AddWithValue("@password",password);
                
                int rows = command.ExecuteNonQuery();
                
                if(rows>0){
                    trans.Commit();
                    return getUserByMail(user.Email);
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            catch (System.Exception)
            {
                try
                {
                    // anything that happend in the above try will get caught in here!
                    trans.Rollback();
                    throw new ArgumentException();
                }
                catch (System.Exception)
                {
                    throw;
                }

            }
            finally
            {
                sqlConnection.Close();
            }
    }

    public IUser getUserByMail(string email)
    {
        IUser user;
            try
            {

                string sqlCmd = "select user_id,email,password from Users where email=@email";
                MySqlDataAdapter adr = new MySqlDataAdapter(sqlCmd, sqlConnection);
                adr.SelectCommand.CommandType = CommandType.Text;
                adr.SelectCommand.Parameters.AddWithValue("@email",email);
                DataTable dt = new DataTable();
                adr.Fill(dt); //opens and closes the DB connection automatically !! (fetches from pool)
                user = dt.AsEnumerable()
                .Select(s => new User(
                
                    s.Field<string>("email"),
                    s.Field<string>("user_id"),
                    s.Field<string>("password")
                )).ToList<IUser>().Find(s => s.Email.Equals(email));;
                if(user==null){
                    throw new ArgumentException();
                }

            }
            catch (System.Exception)
            {
                throw new ArgumentException();
            }
            finally
            {
                sqlConnection.Dispose();
            }

            return user;
    }

    public List<IUser> GetUsers()
    {
        List<IUser> userList;
            try
            {

                string sqlCmd = "select user_id,email,password from Users";
                MySqlDataAdapter adr = new MySqlDataAdapter(sqlCmd, sqlConnection);
                adr.SelectCommand.CommandType = CommandType.Text;
                DataTable dt = new DataTable();
                adr.Fill(dt); //opens and closes the DB connection automatically !! (fetches from pool)
                userList = dt.AsEnumerable()
                .Select(s => new User(
                
                    s.Field<string>("email"),
                    s.Field<string>("user_id"),
                    s.Field<string>("password")
                )).ToList<IUser>();
                

            }
            catch (System.Exception)
            {
                throw;
            }
            finally
            {
                sqlConnection.Dispose();
            }

            return userList;
    }

    public IUser Register(IUserCredentials credentials)
    {
        User user = User.CredentialsToUser(credentials);
        DaoUser daoUser = user.ToDaoUser();
        sqlConnection.Open();
        MySqlTransaction trans = sqlConnection.BeginTransaction();
        try
        {
            MySqlCommand command = new MySqlCommand();
            command.Transaction = trans;
            command.Connection = sqlConnection;
            command.CommandText = @"INSERT INTO Users(user_id,email,password) values(@id,@mail,@pw)";
            command.Parameters.AddWithValue("@id", daoUser.user_id);
            command.Parameters.AddWithValue("@mail", daoUser.email);
            command.Parameters.AddWithValue("@pw", daoUser.password);
            command.ExecuteNonQuery();
            trans.Commit();
            return user;
        }
        catch (System.Exception)
        {
            
            throw new ArgumentException();
        }
        finally{
            sqlConnection.Close();
        }
    }

    public void RemoveUser(IUser user)
    {
        throw new System.NotImplementedException();
    }
}