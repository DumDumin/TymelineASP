using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Tymeline.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {


        private readonly ILogger _logger;
        private readonly IAuthService _authService;

        public AuthController(ILogger<AuthController> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        [HttpPost]
        [Route("register")]
        public ActionResult<IUser> register(UserRegisterCredentials credentials)
        {   
            
            try
            {   
                IUser user = _authService.Register(credentials);
                return StatusCode(201, user);
            }
            catch (System.ArgumentException)
            {
                return StatusCode(200);
            }
            catch (FormatException)
            {
                return StatusCode(400);
            }
        }

        [HttpGet]
        [Route("users")]
        public ActionResult<Int16> ListUsers()
        {
            return StatusCode(200,0);
        }
           
    }
}
