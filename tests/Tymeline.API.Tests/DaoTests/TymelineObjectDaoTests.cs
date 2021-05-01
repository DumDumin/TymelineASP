using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Tymeline.API.Controllers;

namespace Tymeline.API.Tests
{
    public class TymelineObjectDaoTests : OneTimeSetUpAttribute
    {
        Moq.Mock<ILogger<ITymelineObjectDao>> logger;
        Moq.Mock<ITymelineObjectDao> _timelineObjectDao;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _timelineObjectDao = new Moq.Mock<ITymelineObjectDao>();
        }

        [Test]
        public void TestGetAll()
        {
            
        } 
    }
}