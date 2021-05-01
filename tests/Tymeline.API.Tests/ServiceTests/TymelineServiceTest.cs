using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Tymeline.API.Controllers;

namespace Tymeline.API.Tests
{
    public class TymelineServiceTest : OneTimeSetUpAttribute
    {
        ITymelineService _timelineService;
        Moq.Mock<ITymelineObjectDao> _timelineObjectDao;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _timelineObjectDao = new Moq.Mock<ITymelineObjectDao>();
            _timelineService = new TymelineService(_timelineObjectDao.Object);
        }

        [Test]
        public void TestGetAll()
        {   
            _timelineObjectDao.Setup(s => s.getAll()).Returns(new List<TymelineObject>());
            Assert.IsInstanceOf<List<TymelineObject>>(_timelineService.getAll());
        } 
    }
}