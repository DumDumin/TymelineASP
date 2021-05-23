using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

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
            if(user != null){

                // TODO DOMAIN NEEDS TO BE SET BY ENV VARIABLE, just like path in launch setting
                CookieOptions opt = new CookieOptions();
                opt.Domain = "localhost";
                opt.HttpOnly=true;
                opt.Secure=true;
                opt.SameSite=SameSiteMode.Strict;
                opt.MaxAge=TimeSpan.FromHours(12);
                Response.Cookies.Append("jwt",_authService.CreateJWT(user),opt);
                Response.Cookies.Append("asd","testa",opt);
                return StatusCode(201, user);
            }
            else {
                return StatusCode(400, null);
            }
                
            }
            catch (System.Exception)
            {
                return StatusCode(401, null);
            }
           

        }           
    }
}
