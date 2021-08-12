using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Tymeline.API.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class RolesController : Controller
    {
        private IDataRolesService _dataRolesService;
        private readonly ILogger _logger;
        private readonly IAuthService _authService;

        private readonly IJwtService _JwtService;

        public RolesController(ILogger<AuthController> logger, IAuthService authService, IJwtService jwtService, IDataRolesService dataRolesService)
        {
            _dataRolesService = dataRolesService;
            _logger = logger;
            _authService = authService;
            _JwtService = jwtService;
        }

        [Authorize]
        [HttpPost]
        [Route("setroles")]
        public ActionResult<string> SetPermissions([FromBody] HttpUserPermissions userPermissions){
            try
            {
            _dataRolesService.SetUserRoles(userPermissions.toIUserRoles());
            _JwtService.constructJWTHeaders(Response,User.Identity.Name);
            return StatusCode(200);
                
            }
            catch (System.Exception)
            {
                
                return StatusCode(500);
            }
        }

        
        [Authorize]
        [HttpPost]
        [Route("addrole")]
        public ActionResult<string> AddRole( [FromBody] HttpUserPermission userPermission){
            try
            {

            _dataRolesService.AddUserRole(userPermission.ToIUserRole());
            _JwtService.constructJWTHeaders(Response,User.Identity.Name);
            return StatusCode(200);
                
            }
            catch (System.Exception)
            {
                
                return StatusCode(500);
            }
        }


        [Authorize]
        [HttpPost]
        [Route("removerole")]
        public ActionResult<string> RemoveRole( [FromBody] HttpUserPermission userPermission){
            try
            {

            _dataRolesService.RemoveUserRole(userPermission.ToIUserRole());
            _JwtService.constructJWTHeaders(Response,User.Identity.Name);
            return StatusCode(200);
                
            }
            catch (System.Exception)
            {
                return StatusCode(500);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("getroles/{email}")]
        public ActionResult<IUserRoles> GetRole(string email){
            try
            {
            // returns the permissions for some user
            // TODO should only be allowed for certain roles!
            return StatusCode(200,_dataRolesService.GetUserRoles(email));
                
            }
            catch (System.Exception)
            {
                
                return StatusCode(500);
            }
        }


        [HttpGet]
        [Route("userInfo")]
        public ActionResult<IUserRoles> userInfo(){
            
            // returns the permissions for the current user
            return StatusCode(200,_dataRolesService.GetUserRoles(User.Identity.Name));
        }

    }
}