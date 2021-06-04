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
using Newtonsoft.Json.Linq;

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
            return _timelineService.GetAll();
        }
        
        [HttpGet]
        [Route("id/{id}")]
        public ActionResult<TymelineObject> GetTymelineObjectsByKey(string id)
        {   
            
            try
            {
                TymelineObject obj = _timelineService.GetById(id);
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

        [HttpPost]
        [Route("create")]
        public ActionResult<TymelineObject> CreateTymelineObject(TymelineObject tymelineObject)
        {   
            try
            {
                var createdItem = _timelineService.Create(tymelineObject); 
                return StatusCode(createdItem.Item1, createdItem.Item2);
            }
            catch (System.Exception)
            {
                return StatusCode(500);
            }
        }


        [HttpPost]
        [Route("delete")]
        public ActionResult<TymelineObject> DeletetymelineObjectById(string id)
        {   
            try
            {
                _timelineService.DeleteById(id);
                return StatusCode(204);
            }
            catch (System.Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("update")]
        public ActionResult<TymelineObject> UpdateTymelineObjectById(IUpdateTymelineObject data){
            try
            {
            var updatedTymelineObject = _timelineService.UpdateById(data.Id,data.tymelineObject);
                return StatusCode(200,updatedTymelineObject);
            }
            catch (System.Exception)
            {
                
                return StatusCode(500);
            }

        }

    }
}
