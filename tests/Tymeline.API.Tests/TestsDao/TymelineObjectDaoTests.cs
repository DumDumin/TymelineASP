using System.Collections.Generic;
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
        }

        [Test]
        public void TestGetAll()
        {
            Assert.IsInstanceOf<List<TymelineObject>>(_timelineObjectDao.getAll());
        }


        [Test]
        public void TestGetExistingId_Expect_TymelineObject(){
            Assert.IsInstanceOf<TymelineObject>(_timelineObjectDao.getById(2));
        }
        [Test]
        public void GetNotExistingId_Expect_KeyNotFoundException(){

            Assert.Throws<KeyNotFoundException>(() => _timelineObjectDao.getById(3));
        }
    }
}