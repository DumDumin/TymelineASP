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
    public class TymelineController : Controller
    {


        private readonly ILogger _logger;
        private readonly ITymelineService _timelineService;

        public TymelineController( ITymelineService timelineService )
        {
            _timelineService = timelineService;
        }


        [HttpGet]
        [Route("get")]
        public List<TymelineObject> getAllTymelineObjects()
        {
            return _timelineService.GetAll();
        }


        [HttpGet]
        [Route("getbytime/{start}-{end}")]
        public List<TymelineObject> GetByTime(int start, int end)
        {
            return _timelineService.GetByTime(start, end);
        }

        
        [HttpGet]
        [Route("get/{id}")]
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
        public ActionResult<TymelineObject> CreateTymelineObject([FromBody] TymelineObject tymelineObject)
        {   
            try
            {
                return StatusCode(201, _timelineService.Create(tymelineObject));
            }

            catch(System.AccessViolationException)
            {
                return StatusCode(204);
            }
            catch (System.Exception)
            {
                return StatusCode(500);
            }
        }


        [HttpPost]
        [Route("delete")]
        public ActionResult<TymelineObject> DeletetymelineObjectById([FromBody]string id)
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
        public ActionResult<TymelineObject> UpdateTymelineObjectById([FromBody] IUpdateTymelineObject data){
            
            try
            {
                if(data.Id.Equals(data.tymelineObject.Id)){
                    var updatedTymelineObject = _timelineService.UpdateById(data.Id,data.tymelineObject);
                    return StatusCode(200,updatedTymelineObject);

                }
                else{
                    return StatusCode(500,"sent ids dont match");
                }
            }

            
            catch (System.Exception)
            {
                
                return StatusCode(500);
            }
        }
    }
}
