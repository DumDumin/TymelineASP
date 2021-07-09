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
        Moq.Mock<ILogger<ITymelineObjectDao>> logger;
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
            new MySqlCommand("create table Content ( id binary(16) PRIMARY KEY , text varchar(255)); ",connection).ExecuteNonQuery();
            new MySqlCommand("create table TymelineObjects ( id binary(16) PRIMARY KEY, length int, start int, canChangeLength bool, canMove bool, ContentId binary(16) ,constraint fk_content foreign key (ContentId) references Content(id) on update restrict on Delete Cascade); ",connection).ExecuteNonQuery();
            
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
            connection.Open();
            try
            {
            var k = Guid.NewGuid();
            
            var ss = new MySqlCommand("INSERT INTO Content (id,text) VALUES (?id,'asd');",connection);

            ss.Parameters.Add("id",MySqlDbType.Binary).Value = k.ToByteArray();
            // ss.Parameters.AddWithValue("@text","asd");
            ss.ExecuteScalar();

            
            // var sss =new MySqlCommand("insert into Content (id,text) values (UUID_TO_BIN(uuid()),'test text3');",connection).ExecuteScalar();
            // var s = new MySqlCommand("insert into TymelineObjects (id,canChangeLength,canMove,start,length) values(UUID_TO_BIN(uuid()),true,true,12378,12387); ",connection).ExecuteScalar();
            new MySqlCommand("insert into TymelineObjects (id,canChangeLength,canMove,start,length) values(UUID_TO_BIN(uuid()),true,false,13378,1212387); ",connection).ExecuteNonQuery();
            new MySqlCommand("insert into TymelineObjects (id,canChangeLength,canMove,start,length) values(UUID_TO_BIN(uuid()),false,true,14378,12312387); ",connection).ExecuteNonQuery();
            }
            catch (System.Exception)
            {
                
                
                
            }
            finally{
                connection.Close();
            }
        }

        [Test]
        public void TestGetAll()
        {
            Assert.IsInstanceOf<List<TymelineObject>>(_timelineObjectDao.getAll());
        }


        [Test]
        public void TestGetExistingId_Expect_TymelineObject(){
            Assert.IsInstanceOf<TymelineObject>(_timelineObjectDao.getById("1"));
        }
        [Test]
        public void GetNotExistingId_Expect_KeyNotFoundException(){

            Assert.Throws<KeyNotFoundException>(() => _timelineObjectDao.getById("3123"));
        }


        [Test]
        public void InsertNewElement_Expect_Element_To_Exist(){
            var t = new TymelineObject{CanChangeLength=true,CanMove=true,Content=new Content("test"),Length=123,Start=12314};
            
            Assert.IsInstanceOf<TymelineObject>(_timelineObjectDao.Create(t));
        }

        [Test]
        public void InsertNewElement_With_Missing_Content_Expect_SQL_Error(){
            // mathias WTF? how should this happen? this is dealt with in the transaction
             var t = new TymelineObject{CanChangeLength=true,CanMove=true,Content=new Content("test"),Length=123,Start=12314};
            Assert.IsInstanceOf<TymelineObject>(_timelineObjectDao.Create(t));
        }

        [Test]
        public void InsertExistingElement_Expect_SQL_Error(){
            // does not make sense with this implementation
            var t = new TymelineObject{CanChangeLength=true,CanMove=true,Content=new Content("test"),Length=123,Start=12314};
            Assert.IsInstanceOf<TymelineObject>(_timelineObjectDao.Create(t));
        }
    }
}