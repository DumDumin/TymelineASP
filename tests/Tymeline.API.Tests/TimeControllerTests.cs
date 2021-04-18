using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Tymeline.API.Controllers;

namespace Tymeline.API.Tests
{
    public class Tests : OneTimeSetUpAttribute
    {
        Moq.Mock<ILogger<TimeController>> logger;
        Moq.Mock<ITimeService> timeService;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            logger = new Moq.Mock<ILogger<TimeController>>();
            timeService = new Moq.Mock<ITimeService>();
        }

        [Test]
        public async Task Test_Dependency()
        {
            var controller = new Tymeline.API.Controllers.TimeController(logger.Object, timeService.Object);
        } 
    }
}