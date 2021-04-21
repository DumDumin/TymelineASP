using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Tymeline.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Route("/util/time")]
    public class TimeController : Controller
    {


        private readonly ILogger _logger;
        private readonly ITimeService _timeService;

        public TimeController(ILogger<TimeController> logger, ITimeService timeService)
        {
            _logger = logger;
            _timeService = timeService;
        }

        [HttpGet]
        // [Route("time")]
        // [Route("~/dumm/time")]
        public Int32 GetTime()
        {
        Console.WriteLine("5");
          return _timeService.GetTime();
        }
    }
}
