using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
        public ActionResult<string> SetPermissions([FromBody] HttpUserRoles userPermissions)
        {
            try
            {
                _dataRolesService.SetUserRoles(userPermissions.toIUserRoles());
                _JwtService.constructJWTHeaders(Response, User.Identity.Name);
                return StatusCode(200);

            }
            catch (System.Exception)
            {

                return StatusCode(500);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("getroles/email/{email}")]
        public ActionResult<HttpUserRoles> GetRoleByEmail(string email)
        {
            try
            {
                // returns the permissions for some user
                // TODO should only be allowed for certain roles!
                IUserRoles s = _dataRolesService.GetUserRoles(email);
                var returnObject = new HttpUserRoles(s.Email, s.Roles.ConvertAll(o => (Role)o));
                return StatusCode(200, returnObject);
            }
            catch (System.Exception)
            {

                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpGet]
        [Route("getroles/item/{item}")]
        public ActionResult<HttpTymelineObjectRoles> GetRoleByItem(string item)
        {
            try
            {
                // returns the permissions for some item
                ITymelineObjectRoles s = _dataRolesService.GetItemRoles(item);
                HttpTymelineObjectRoles returnObject = new HttpTymelineObjectRoles { tymelineObjectId = s.TymelineObject.Id, Roles = s.Roles.ConvertAll(o => (Role)o) };
                return StatusCode(200, returnObject);
            }
            catch (System.Exception)
            {
                return StatusCode(500);
            }
        }


        [Authorize]
        [HttpGet]
        [Route("getroles")]
        public ActionResult<List<IRole>> GetRoles()
        {
            try
            {
                // returns the permissions for some user
                // TODO should only be allowed for certain roles!
                return StatusCode(200, _dataRolesService.GetRoles());
            }
            catch (System.Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("userInfo")]
        public ActionResult<IUserRoles> userInfo()
        {
            // returns the permissions for the current user
            return StatusCode(200, _dataRolesService.GetUserRoles(User.Identity.Name));
        }

        [Authorize]
        [HttpPost]
        [Route("additemrole")]
        public ActionResult<HttpTymelineObjectRoles> ItemRoleAdd([FromBody] HttpTymelineObjectRolesIncrement roleIncrement)
        {

            try
            {
                List<IRole> newRoles = _dataRolesService.AddRoleToItem(roleIncrement.Role, roleIncrement.tymelineObjectId);
                return StatusCode(200, new HttpTymelineObjectRoles { Roles = newRoles.ConvertAll(o => (Role)o), tymelineObjectId = roleIncrement.tymelineObjectId });
            }
            catch (System.Exception)
            {
                return StatusCode(500);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("removeitemrole")]
        public ActionResult<HttpTymelineObjectRoles> ItemRoleRemove([FromBody] HttpTymelineObjectRolesIncrement roleIncrement)
        {
            try
            {
                List<IRole> newRoles = _dataRolesService.RemoveRoleFromItem(roleIncrement.Role, roleIncrement.tymelineObjectId);
                return StatusCode(200, new HttpTymelineObjectRoles { Roles = newRoles.ConvertAll(o => (Role)o), tymelineObjectId = roleIncrement.tymelineObjectId });
            }
            catch (System.Exception)
            {
                return StatusCode(500);
            }
        }


        [Authorize]
        [HttpPost]
        [Route("addroletouser")]
        public ActionResult<List<Role>> AddRoleToUser([FromBody] HttpUserRole userPermission)
        {
            try
            {
                List<IRole> roles = _dataRolesService.AddUserRole(userPermission.ToIUserRole());
                _JwtService.constructJWTHeaders(Response, User.Identity.Name);
                return StatusCode(200, roles.ConvertAll(o => (Role)o));
            }
            catch (System.Exception)
            {
                return StatusCode(500);
            }
        }


        [Authorize]
        [HttpPost]
        [Route("removerolefromuser")]
        public ActionResult<string> RemoveRole([FromBody] HttpUserRole userPermission)
        {
            try
            {
                List<IRole> roles = _dataRolesService.RemoveUserRole(userPermission.ToIUserRole());
                _JwtService.constructJWTHeaders(Response, User.Identity.Name);
                return StatusCode(200, roles.ConvertAll(o => (Role)o));
            }
            catch (System.Exception)
            {
                return StatusCode(500);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("addrole")]
        public ActionResult<string> AddRole([FromBody] Role userPermission)
        {
            try
            {
                _dataRolesService.AddRole(userPermission);
                _JwtService.constructJWTHeaders(Response, User.Identity.Name);
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
        public ActionResult<string> RemoveRole([FromBody] Role userPermission)
        {
            try
            {
                _dataRolesService.RemoveRole(userPermission);
                _JwtService.constructJWTHeaders(Response, User.Identity.Name);
                return StatusCode(200);
            }
            catch (System.Exception)
            {
                return StatusCode(500);
            }
        }

    }
}