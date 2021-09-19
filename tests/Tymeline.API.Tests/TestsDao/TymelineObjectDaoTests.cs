using System;
using System.Collections.Generic;
using System.IO;
using AutoFixture.NUnit3;
using FluentAssertions;
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
            TestUtil.setupDB(mySqlConnection);
            var items = TestUtil.prepopulateTymelineObjects(mySqlConnection);
            var users = TestUtil.prepopulateUser(mySqlConnection);
            var roles = TestUtil.prepopulateRoles(mySqlConnection);
            TestUtil.prepopulateUserRoles(mySqlConnection,roles,users);
            TestUtil.prepopulateItemRoles(mySqlConnection,roles,items);
            TestUtil.EnsureSuccessfullSetup(mySqlConnection);
        }



       

        [Test]
        [Category("Sql")]
        public void TestGetAll()
        {
            Assert.IsInstanceOf<List<TymelineObject>>(_timelineObjectDao.getAll());
        }


        [Test]
        [Category("Sql")]
        public void Test_Get_Existing_Id_Expect_TymelineObject(){
            var first = _timelineObjectDao.getAll()[0];
            Assert.IsInstanceOf<TymelineObject>(_timelineObjectDao.getById(first.Id));
        }
        [Test]
        [Category("Sql")]
        public void Test_Get_Not_Existing_Id_Expect_ArgumentException(){

            Assert.Throws<ArgumentException>(() => _timelineObjectDao.getById("3123"));
        }


        [Test]
        [Category("Sql")]
        public void Test_Insert_New_Element_Expect_Element_To_Exist(){
            var t = new TymelineObject{CanChangeLength=true,CanMove=true,Content=new Content("test"),Length=123,Start=12314,Id="999112"};
            
            Assert.IsInstanceOf<TymelineObject>(_timelineObjectDao.Create(t));
        }

        [Test]
        [Category("Sql")]
        public void Test_Insert_New_Element_With_Missing_Content_Expect_SQL_Error(){
           
            var t = new TymelineObject{CanChangeLength=true,CanMove=true,Length=123,Start=12314,Id="999113"};
            Assert.Throws<ArgumentException>(() => _timelineObjectDao.Create(t));
            
        }

        [Test]
        [Category("Sql")]
        public void Test_Insert_Existing_Element_Expect_ArgumentException(){
            
            var t = new TymelineObject{CanChangeLength=true,CanMove=true,Content=new Content("test"),Length=123,Start=12314,Id="999114"};
            Assert.IsInstanceOf<TymelineObject>(_timelineObjectDao.Create(t));
            Assert.Throws<ArgumentException>(() => _timelineObjectDao.Create(t));
        }


        [Test,AutoData]
        [Category("Sql")]
        public void Test_Update_Not_Existing_Id_Expect_ArgumentException(string id){
            
            var t = new TymelineObject{CanChangeLength=true,CanMove=true,Content=new Content("test"),Length=123,Start=12314,Id=id};
           ;
            Action act = () => _timelineObjectDao.UpdateById(t.Id,t);
            act.Should().Throw<ArgumentException>();
        }


        [Test,AutoData]
        [Category("Sql")]
        public void Test_Update_Content_ExistingId_Expect_ItemUpdated(string id){
            var t = new TymelineObject{CanChangeLength=true,CanMove=true,Content=new Content("test"),Length=123,Start=12314,Id=id};
            _timelineObjectDao.Create(t);

            t.Content.Text="yolo";
            _timelineObjectDao.UpdateById(id,t);
            _timelineObjectDao.getById(t.Id).Should().Be(t);

        }
        
        [Test,AutoData]
        [Category("Sql")]
        public void Test_Update_TymelineObject_ExistingId_Expect_ItemUpdated(string id){
            var t = new TymelineObject{CanChangeLength=true,CanMove=true,Content=new Content("test"),Length=123,Start=12314,Id=id};
            _timelineObjectDao.Create(t);
            t.Length=1234;
            _timelineObjectDao.UpdateById(id,t);
            _timelineObjectDao.getById(t.Id).Should().Be(t);

        }


        [Test,AutoData]
        [Category("Sql")]
        public void Test_Update_Wrong_Id_Expect_ArgumentException(string realId,string fakeId){
            var t = new TymelineObject{CanChangeLength=true,CanMove=true,Content=new Content("test"),Length=123,Start=12314,Id=realId};
            _timelineObjectDao.Create(t);
            t.Length=1234;
            Action act = () =>_timelineObjectDao.UpdateById(fakeId,t);
            act.Should().Throw<ArgumentException>();
        }


        [Test,AutoData]
        [Category("Sql")]
        public void Test_Delete_Id_Expect_Item_To_Be_Deleted(string realId){
            var t = new TymelineObject{CanChangeLength=true,CanMove=true,Content=new Content("test"),Length=123,Start=12314,Id=realId};
            _timelineObjectDao.Create(t);
            
            _timelineObjectDao.DeleteById(realId);
            Action act = () => _timelineObjectDao.getById(realId);
            act.Should().Throw<ArgumentException>();
        }


        [Test,AutoData]
        [Category("Sql")]
        public void Test_Delete_Id_Twice_Expect_Item_To_Be_Deleted(string realId){
            var t = new TymelineObject{CanChangeLength=true,CanMove=true,Content=new Content("test"),Length=123,Start=12314,Id=realId};
            _timelineObjectDao.Create(t);
            
            _timelineObjectDao.DeleteById(realId);
            Action actdelete = () => _timelineObjectDao.DeleteById(realId);
            actdelete.Should().NotThrow();
            Action act = () => _timelineObjectDao.getById(realId);
            act.Should().Throw<ArgumentException>();
        }


    }
}