using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;






public class JwtService : IJwtService
{


    private IDataRolesService _rolesService;
    private readonly AppSettings _appSettings;
    public JwtService(IDataRolesService rolesService,IOptions<AppSettings> appSettings){
        _appSettings = appSettings.Value;
        _rolesService = rolesService;
    }
    
    
    public HttpResponse constructJWTHeaders(HttpResponse Response, string mail)
        {
            // TODO DOMAIN NEEDS TO BE SET BY ENV VARIABLE, just like path in launch setting
            CookieOptions opt = new CookieOptions();
            opt.Domain = "localhost";
            opt.HttpOnly = true;
            opt.Secure = true;
            opt.SameSite = SameSiteMode.Strict;
            opt.MaxAge = TimeSpan.FromHours(12);
            Response.Cookies.Append("jwt", createJwt(mail), opt);
            return Response;
        }
    public string createJwt(string userMail)
    {
        List<IRole> userRoles = _rolesService.GetUserRoles(userMail).Roles.Where(permission => permission.Type.Equals("Frontend")).ToList();


        var utcNow = DateTime.UtcNow; 
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
                Subject = new ClaimsIdentity(new Claim[] 
                {
                    //add new claims in here
                    
  
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = _appSettings.Hostname,
                Issuer = _appSettings.Hostname,
            };

        var claimlist = new List<Claim>();
        userRoles.ForEach(permission => claimlist.Add(new Claim(permission.Type,permission.Value)));
        claimlist.Add(new Claim(ClaimTypes.Name, userMail));
        claimlist.Add(new Claim(ClaimTypes.Role, "admin"));
        tokenDescriptor.Subject.AddClaims(claimlist);


        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        return tokenHandler.WriteToken(token);
    }


    public JwtSecurityToken verifyJwt(string token)
    {
         try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = _appSettings.Hostname,
                    ValidIssuer = _appSettings.Hostname,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                
                return jwtToken;
            }
            catch
            {   
                throw new System.NotImplementedException();
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
            
    }
}