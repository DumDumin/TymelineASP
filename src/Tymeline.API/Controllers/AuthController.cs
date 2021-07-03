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

        private void constructLoginHeaders(IUser user)
        {
            // TODO DOMAIN NEEDS TO BE SET BY ENV VARIABLE, just like path in launch setting
            CookieOptions opt = new CookieOptions();
            opt.Domain = "localhost";
            opt.HttpOnly = true;
            opt.Secure = true;
            opt.SameSite = SameSiteMode.Strict;
            opt.MaxAge = TimeSpan.FromHours(12);
            Response.Cookies.Append("jwt", _JwtService.createJwt(user), opt);
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
                constructLoginHeaders(user);
                return StatusCode(201, user);
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

        [HttpGet]
        [Route("userInfo")]
        public ActionResult<IUserPermissions> userInfo(){
            

            return StatusCode(200,_authService.GetUserPermissions(User.Identity.Name));
        }


    }
}
