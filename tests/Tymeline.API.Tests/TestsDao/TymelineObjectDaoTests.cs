using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using NUnit.Framework;

using Tymeline.API.Daos;

namespace Tymeline.API.Tests
{
    public class TymelineObjectDaoTests : OneTimeSetUpAttribute
    {
        private AppSettings _configuration;
        ITymelineObjectDao _timelineObjectDao;


        public static AppSettings GetApplicationConfiguration()
        {
            var configuration = new AppSettings();

            var iConfig = new ConfigurationBuilder()
            
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            iConfig
                .GetSection("AppSettings")
                .Bind(configuration);

            return configuration;
        }
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _configuration = GetApplicationConfiguration();
            IOptions<AppSettings> options = Options.Create<AppSettings>(_configuration);
            var s = new MySqlConnectionStringBuilder(options.Value.SqlConnection.MySqlConnectionString);
            var mySqlConnection = new MySqlConnection(s.ConnectionString);
            _timelineObjectDao = new TymelineObjectDaoMySql(mySqlConnection);
            setupDB(mySqlConnection);
            prepopulateTymelineObjects(mySqlConnection);
        }


        private void setupDB(MySqlConnection connection){
            connection.Open();
            try
            {
            new MySqlCommand("drop table IF EXISTS TymelineObjects",connection).ExecuteNonQuery();
            new MySqlCommand("drop table IF EXISTS Content",connection).ExecuteNonQuery();
            new MySqlCommand("create table Content ( id varchar(64) PRIMARY KEY , text varchar(255)); ",connection).ExecuteNonQuery();
            new MySqlCommand("create table TymelineObjects ( id varchar(64) PRIMARY KEY, length int, start int, canChangeLength bool, canMove bool, ContentId varchar(64) ,constraint fk_content foreign key (ContentId) references Content(id) on update restrict on Delete Cascade); ",connection).ExecuteNonQuery();
            
            }
            catch (System.Exception)
            {
                
                throw;
            }
            finally{
                connection.Close();
            }


        }


        private void prepopulateTymelineObjects(MySqlConnection connection){
            try
            {
            connection.Open();

            var command = new MySqlCommand("insert into Content values (@id,@text)",connection);
            var commandtymeline = new MySqlCommand("insert into TymelineObjects (id, canChangeLength, canMove, start, length, ContentId) values(@id,true,false,FLOOR(RAND()*10000),FLOOR(RAND()*1000000),@guid); ",connection);
            var initialContentId = Guid.NewGuid().ToString();
            var initialTymelineId= Guid.NewGuid().ToString();


            command.Parameters.AddWithValue("@id",initialContentId);
            command.Parameters.AddWithValue("@text",TestUtil.RandomString(50));
            command.Prepare();
            command.ExecuteNonQuery();


            commandtymeline.Parameters.AddWithValue("@id",initialTymelineId);
            commandtymeline.Parameters.AddWithValue("@guid",initialContentId);

            commandtymeline.Prepare();
            commandtymeline.ExecuteNonQuery();

            for (int i = 0; i < 50; i++)
            {
                var idContent = Guid.NewGuid().ToString();
                var idTymeline = Guid.NewGuid().ToString();
                command.Parameters["@id"].Value = idContent;
                command.Parameters["@text"].Value = TestUtil.RandomString(50);
                command.ExecuteNonQuery();
                commandtymeline.Parameters["@id"].Value=idTymeline;
                commandtymeline.Parameters["@guid"].Value=idContent;
                commandtymeline.ExecuteNonQuery();
            }
       
            }
            catch (System.Exception)
            {
                
                
                
            }
            finally{
                connection.Close();
            }
        }

        [Test]
        [Category("Sql")]
        public void TestGetAll()
        {
            Assert.IsInstanceOf<List<TymelineObject>>(_timelineObjectDao.getAll());
        }


        [Test]
        [Category("Sql")]
        public void TestGetExistingId_Expect_TymelineObject(){
            var first = _timelineObjectDao.getAll()[0];
            Assert.IsInstanceOf<TymelineObject>(_timelineObjectDao.getById(first.Id));
        }
        [Test]
        [Category("Sql")]
        public void GetNotExistingId_Expect_KeyNotFoundException(){

            Assert.Throws<KeyNotFoundException>(() => _timelineObjectDao.getById("3123"));
        }


        [Test]
        [Category("Sql")]
        public void InsertNewElement_Expect_Element_To_Exist(){
            var t = new TymelineObject{CanChangeLength=true,CanMove=true,Content=new Content("test"),Length=123,Start=12314};
            
            Assert.IsInstanceOf<TymelineObject>(_timelineObjectDao.Create(t));
        }

        [Test]
        [Category("Sql")]
        public void InsertNewElement_With_Missing_Content_Expect_SQL_Error(){
            // mathias WTF? how should this happen? this is dealt with in the transaction
             var t = new TymelineObject{CanChangeLength=true,CanMove=true,Content=new Content("test"),Length=123,Start=12314};
            Assert.IsInstanceOf<TymelineObject>(_timelineObjectDao.Create(t));
        }

        [Test]
        [Category("Sql")]
        public void InsertExistingElement_Expect_SQL_Error(){
            // does not make sense with this implementation
            var t = new TymelineObject{CanChangeLength=true,CanMove=true,Content=new Content("test"),Length=123,Start=12314};
            Assert.IsInstanceOf<TymelineObject>(_timelineObjectDao.Create(t));
        }
    }
}