using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
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
            context.Items["User"] = new User(_authTestSettings.userEmail, _authTestSettings.userPassword);

            await _next(context);
        }

    
        
    }
