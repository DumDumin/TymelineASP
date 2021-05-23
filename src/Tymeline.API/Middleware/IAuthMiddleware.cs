using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public interface IAuthMiddleware{
    public Task Invoke(HttpContext context, IAuthService userService);

}