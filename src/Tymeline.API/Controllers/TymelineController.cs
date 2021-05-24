using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Tymeline.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    // [Authorize]
    public class TymelineController : Controller
    {


        private readonly ILogger _logger;
        private readonly ITymelineService _timelineService;

        public TymelineController( ITymelineService timelineService )
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
            
            try
            {
                TymelineObject obj = _timelineService.getById(id);
                return StatusCode(200,obj);
                
            }
            catch (ArgumentException)
            {
                return  StatusCode(500);
            }
            catch(KeyNotFoundException)
            {
                return StatusCode(204, null);
            }
            
        }


    }
}
