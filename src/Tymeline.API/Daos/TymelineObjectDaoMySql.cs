using System.Collections.Generic;
using System.Transactions;
using System.Data;

using MySql.Data;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace Tymeline.API.Daos
{
    public class TymelineObjectDaoMySql : ITymelineObjectDao
    {

        MySqlConnection sqlConnection;

        public TymelineObjectDaoMySql(MySqlConnection connection)
        {
            sqlConnection = connection;
        }

        public TymelineObject getTymelineObjects()
        {
            throw new System.NotImplementedException();
        }

        public List<TymelineObject> getAll()
        {

            List<TymelineObject> tymelineList;
            try
            {

                string sqlCmd = "select t.id,t.start,t.length,t.canMove,t.canChangeLength,c.text from TymelineObjects t inner join Content c on c.fk_tymeline = t.ContentID";
                MySqlDataAdapter adr = new MySqlDataAdapter(sqlCmd, sqlConnection);
                adr.SelectCommand.CommandType = CommandType.Text;
                DataTable dt = new DataTable();
                adr.Fill(dt); //opens and closes the DB connection automatically !! (fetches from pool)
                tymelineList = dt.AsEnumerable()
                .Select(s => new TymelineObject()
                {
                    Start = s.Field<int>("start"),
                    Id = s.Field<string>("id"),
                    Length = s.Field<int>("length"),
                    Content = new Content(s.Field<string>("text")),
                    CanMove = s.Field<bool>("canMove"),
                    CanChangeLength = s.Field<bool>("canChangeLength")
                })
                .ToList();

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                sqlConnection.Dispose();
            }

            return tymelineList;

        }

        public TymelineObject getById(string id)
        {
            TymelineObject tymelineObject;
            try
            {

                string sqlCmd = @"
                select timeline.id,
                timeline.start,
                timeline.length,
                timeline.canMove,
                timeline.canChangeLength,
                content.text 
                from TymelineObjects timeline inner join `Content` content on content.fk_tymeline = timeline.ContentID where timeline.id = @id";
                MySqlDataAdapter adr = new MySqlDataAdapter(sqlCmd, sqlConnection);
                adr.SelectCommand.Parameters.AddWithValue("@id", id);
                adr.SelectCommand.CommandType = CommandType.Text;
                DataTable dt = new DataTable();
                adr.Fill(dt); //opens and closes the DB connection automatically !! (fetches from pool)
                tymelineObject = dt.AsEnumerable()
                .Select(s => new TymelineObject()
                {
                    Start = s.Field<int>("start"),
                    Id = s.Field<string>("id"),
                    Length = s.Field<int>("length"),
                    Content = new Content(s.Field<string>("text")),
                    CanMove = s.Field<bool>("canMove"),
                    CanChangeLength = s.Field<bool>("canChangeLength")
                })
                .ToList()
                .Find(s => s.Id.Equals(id));

                if (tymelineObject == null)
                {
                    throw new ArgumentException();
                }

            }
            catch (Exception)
            {
                throw new ArgumentException();
            }
            finally
            {
                sqlConnection.Dispose();
            }

            return tymelineObject;
        }

        public List<TymelineObject> getByTime(int start, int end)
        {
            throw new System.NotImplementedException();
        }


        public void DeleteById(string id)
        {
            sqlConnection.Open();
            MySqlTransaction trans = sqlConnection.BeginTransaction();
            try
            {
                MySqlCommand command = new MySqlCommand();
                command.Transaction = trans;
                command.Connection = sqlConnection;
                command.CommandText = "DELETE FROM TymelineObjects where id=@id";
                command.Parameters.AddWithValue("@id", id);

                var reader = command.ExecuteReader();
                reader.Close();
                trans.Commit();
                
            }
            catch (System.Exception)
            {
                try
                {
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

        public TymelineObject Create(TymelineObject tymelineObject)
        {
            sqlConnection.Open();
            MySqlTransaction trans = sqlConnection.BeginTransaction();
            try
            {
                MySqlCommand command = new MySqlCommand();
                command.Transaction = trans;
                command.Connection = sqlConnection;
                var textGuid = Guid.NewGuid().ToString();
         

                command.CommandText = @"INSERT INTO  TymelineObjects (id,length,start,canChangeLength,canMove,contentId)
                values(@id,@length,@start,@canChangeLength,@canMove,@contentId)";

                command.Parameters.AddWithValue("@id", tymelineObject.Id);
                command.Parameters.AddWithValue("@length", tymelineObject.Length);
                command.Parameters.AddWithValue("@start", tymelineObject.Start);
                command.Parameters.AddWithValue("@canChangeLength", tymelineObject.CanChangeLength);
                command.Parameters.AddWithValue("@canMove", tymelineObject.CanMove);
                command.Parameters.AddWithValue("@contentId", textGuid);
                command.ExecuteNonQuery();
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO Content(fk_tymeline,text) values(@fk_tymeline,@text)";
                command.Parameters.AddWithValue("@fk_tymeline", textGuid);
                command.Parameters.AddWithValue("@text", tymelineObject.Content.Text);
                command.ExecuteNonQuery();

                trans.Commit();
                return getById(tymelineObject.Id);
            }
            catch (System.Exception)
            {
                try
                {
                    trans.Rollback();
                    throw new ArgumentException("Most likely the object was alread registered");
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

        public TymelineObject UpdateById(string id, TymelineObject tymelineObject)
        {
            // opt out if the ids dont match!
            // THE ID IS JUST PASSED DOWN FOR CONVENIENCE
            // DEPRECATE THE USE OF id in this method!
            if (!id.Equals(tymelineObject.Id))
            {
                throw new ArgumentException();
            }
            sqlConnection.Open();
            MySqlTransaction trans = sqlConnection.BeginTransaction();
            try
            {

                MySqlCommand command = new MySqlCommand();
                command.Transaction = trans;
                command.Connection = sqlConnection;
                command.CommandText = @"Update Content c Inner Join TymelineObjects t ON (c.fk_tymeline=t.contentId)
                SET c.text=@text,
                t.length=@length,
                t.start=@start,
                t.canChangeLength=@canChangeLength,
                t.canMove=@canMove
                where t.id=@id
                ";
                command.Parameters.AddWithValue("@id", tymelineObject.Id);
                command.Parameters.AddWithValue("@length", tymelineObject.Length);
                command.Parameters.AddWithValue("@start", tymelineObject.Start);
                command.Parameters.AddWithValue("@canChangeLength", tymelineObject.CanChangeLength);
                command.Parameters.AddWithValue("@canMove", tymelineObject.CanMove);
                command.Parameters.AddWithValue("@text", tymelineObject.Content.Text);
                int rows = command.ExecuteNonQuery();
                
                if(rows>0){
                    trans.Commit();
                    return getById(tymelineObject.Id);
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
    }
}