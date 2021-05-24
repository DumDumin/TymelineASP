using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;



    public class TestAuthenticationMiddleware:IAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;

    private readonly CustomAuthenticationOptions _authTestSettings;

    public TestAuthenticationMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings, IOptions<CustomAuthenticationOptions> authTestSettings)
        {
            _next = next;
            _appSettings = appSettings.Value;
            _authTestSettings = authTestSettings.Value;
        }

        public async Task Invoke(HttpContext context, IAuthService userService)
        {
            context.Items["User"] = new User("test@testmail.de","testpasswd");

            await _next(context);
        }

    
        
    }
