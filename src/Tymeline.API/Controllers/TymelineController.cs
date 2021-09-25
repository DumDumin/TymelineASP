using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
        private readonly IDataRolesService _rolesService;

        public TymelineController( ITymelineService timelineService, IDataRolesService rolesService )
        {
            _timelineService = timelineService;
            _rolesService = rolesService;
        }

        [Authorize]
        [HttpGet]
        [Route("get")]
        public List<TymelineObject> getAllTymelineObjects()
        {
            return _timelineService.GetAll();
        }

        [Authorize]
        [HttpGet]
        [Route("getbytime/{start}-{end}")]
        public List<TymelineObject> GetByTime(int start, int end)
        {
            return _timelineService.GetByTime(start, end);
        }

        [Authorize]
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
                return  StatusCode(204,null);
            }
            catch(System.Exception)
            {
                return StatusCode(500);
            }
            
        }

        [Authorize]
        [HttpPost]
        [Route("create")]
        public ActionResult<TymelineObject> CreateTymelineObject([FromBody] HttpTymelineObjectWithRole tymelineObjectWithRole)
        {   
            try
            {


                List<Claim> ToClaims = tymelineObjectWithRole.Roles.ConvertAll<Claim>(role => new Claim(role.Type,role.Value));  
                // User.Claims.Contains()
                // check if User has Permissions to use the Roles
                // reject if not
                // reject if roles are not created!
                // create item
                // assign item to roles!

                return StatusCode(201, _timelineService.Create(tymelineObjectWithRole.tymelineObject));
            }
            catch (System.Exception)
            {
                return StatusCode(500);
            }
        }


        [Authorize]
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

        [Authorize]
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
