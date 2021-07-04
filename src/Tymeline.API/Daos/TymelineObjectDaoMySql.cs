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
    public class TymelineObjectDaoMySql: ITymelineObjectDao{

        MySqlConnection sqlConnection;

        public TymelineObjectDaoMySql(MySqlConnection connection){
           sqlConnection = connection;
        }

        public TymelineObject getTymelineObjects(){
            throw new System.NotImplementedException();
        }    

        public List<TymelineObject> getAll(){

            List<TymelineObject> tymelineList;
            try{
                
                string sqlCmd = "select t.id,t.start,t.length,t.`canMove`,t.`canChangeLength`,c.text  from `TymelineObjects` t inner join `Content` c on c.id = t.`ContentID`";
                // string sqlCmd = "select t.id,t.start,t.`canMove`,t.`canChangeLength`,t.`start`,c.text  from `TymelineObjects` t inner join `Content` c on c.id = t.`ContentID`";
                MySqlDataAdapter adr = new MySqlDataAdapter(sqlCmd, sqlConnection);
                adr.SelectCommand.CommandType = CommandType.Text;
                DataTable dt = new DataTable();
                adr.Fill(dt); //opens and closes the DB connection automatically !! (fetches from pool)
                tymelineList = dt.AsEnumerable()
                .Select(s => new TymelineObject()
                {Start= s.Field<int>("start"),
                Id=s.Field<int>("id"), Length=s.Field<int>("length"),
                Content=new Content(s.Field<string>("text")),
                CanMove=s.Field<bool>("canMove"),
                CanChangeLength=s.Field<bool>("canChangeLength") })
                .ToList();

            }catch (DataException){
                throw;
            }
            catch(Exception){
                throw new KeyNotFoundException();
            }finally{
                sqlConnection.Dispose();
            }

            return tymelineList;
            
        }

        public TymelineObject getById(int id)
        {
            TymelineObject tymelineObject;
            try{
                
                string sqlCmd = $@"
                select timeline.id,
                timeline.start,
                timeline.length,
                timeline.canMove,
                timeline.canChangeLength,
                content.text 
                from TymelineObjects timeline inner join `Content` content on content.id = timeline.ContentID where timeline.id = {id}";
              
                MySqlDataAdapter adr = new MySqlDataAdapter(sqlCmd, sqlConnection);
                adr.SelectCommand.CommandType = CommandType.Text;
                DataTable dt = new DataTable();
                adr.Fill(dt); //opens and closes the DB connection automatically !! (fetches from pool)
                tymelineObject = dt.AsEnumerable()
                .Select(s => new TymelineObject()
                {Start= s.Field<int>("start"),
                Id=s.Field<int>("id"),
                Length=s.Field<int>("length"),
                Content=new Content(s.Field<string>("text")),
                CanMove=s.Field<bool>("canMove"),
                CanChangeLength=s.Field<bool>("canChangeLength") })
                .ToList()
                .Find(s => s.Id.Equals(id));

                if(tymelineObject == null){
                    throw new KeyNotFoundException();
                }

            }catch (Exception){
                throw new KeyNotFoundException();
            }finally{
                sqlConnection.Dispose();
            }

            return tymelineObject;
        }

        public List<TymelineObject> getByTime(int start, int end)
        {
            throw new System.NotImplementedException();
        }


        public void DeleteById(int id){
            throw new System.NotImplementedException();
        }

        public TymelineObject Create(TymelineObject tymelineObject)
        {
            throw new System.NotImplementedException();
        }

        public TymelineObject UpdateById(int id, TymelineObject tymelineObject)
        {
            throw new System.NotImplementedException();
        }
    }
}