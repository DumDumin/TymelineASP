using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;

public interface IJwtService{
    HttpResponse constructJWTHeaders(HttpResponse Response, string mail);
    string createJwt(string userMail);
    JwtSecurityToken verifyJwt(string jwt);
}