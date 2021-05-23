using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Tymeline.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class TymelineController : Controller
    {


        private readonly ILogger _logger;
        private readonly ITymelineService _timelineService;

        public TymelineController( ITymelineService timelineService)
        {
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


    }
}
