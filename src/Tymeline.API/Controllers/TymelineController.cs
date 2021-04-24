using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Tymeline.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TymelineController : Controller
    {


        private readonly ILogger _logger;
        private readonly ITymelineService _timelineService;

        public TymelineController(ILogger<TimeController> logger, ITymelineService timelineService)
        {
            _logger = logger;
            _timelineService = timelineService;
        }

        [HttpGet]
        [Route("all")]
        public List<TymelineObject> getAllTymelineObjects()
        {
            return _timelineService.getAll();
        }

        [HttpGet]
        [Route("id/{id}")]
        public ActionResult<TymelineObject> GetTymelineObjectsByKey(string id)
        {   
            ActionResult<TymelineObject> returnvalue;
            try
            {
                TymelineObject obj = _timelineService.getById(id);
                returnvalue = StatusCode(200,obj);
                
            }
            catch (ArgumentException)
            {
                returnvalue = StatusCode(500);
            }
            catch(KeyNotFoundException)
            {
                returnvalue = StatusCode(204, null);
            }
            return returnvalue;
            
        }

        private ActionResult<TymelineObject> produceTymelineRESTResponse(TymelineObject tymelineObject)
        {
            if (tymelineObject == null)
            {
                return StatusCode(204, null);
            }
            else
            {
                return StatusCode(200, tymelineObject);
            }
        }
    }
}
