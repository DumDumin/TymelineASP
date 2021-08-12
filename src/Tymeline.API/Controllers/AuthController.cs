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
    public class AuthController : Controller
    {


        private readonly ILogger _logger;
        private readonly IAuthService _authService;

        private readonly IJwtService _JwtService;

        public AuthController(ILogger<AuthController> logger, IAuthService authService, IJwtService jwtService)
        {
            _logger = logger;
            _authService = authService;
            _JwtService = jwtService;
        }


        [HttpPost]
        [Route("register")]
        public ActionResult<IUser> register(UserCredentials credentials)
        {   
            
            try
            {   
                IUser user = _authService.Register(credentials);
                if(user!=null){
                return StatusCode(201, JsonConvert.SerializeObject(user));

                }
                return StatusCode(400,"");
            }
            catch (System.Exception)
            {
                return StatusCode(400,"");
            }
           
        }

        [Authorize]
        [HttpGet]
        [Route("users")]
        public ActionResult<List<IUser>> ListUsers()
        {
            List<IUser> users = _authService.getUsers();
            return StatusCode(200,users);
        }

        [HttpPost]
        [Route("login")]
        public ActionResult<IUser> Login(UserCredentials credentials)
        {
            try
            {
                IUser user = _authService.Login(credentials);
                _JwtService.constructJWTHeaders(Response,user.Email);
                
                return StatusCode(200, user);
            }
             catch (ArgumentException)
            {
                return StatusCode(400, null);
            }
            catch (System.Exception)
            {
                return StatusCode(401, null);
            }
           

        }

        [Authorize]
        [HttpGet]
        [Route("testjwt")]
        public ActionResult<string> TestJWT(){

            return StatusCode(200,User.Claims.ToString());
        }






        

        
    }
}
