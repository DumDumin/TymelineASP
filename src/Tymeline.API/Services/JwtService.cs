using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;






public class JwtService : IJwtService
{

    private readonly AppSettings _appSettings;
    public JwtService(IOptions<AppSettings> appSettings){
        _appSettings = appSettings.Value;
    }
    

    public string createJwt(IUser user)
    {
        var utcNow = DateTime.UtcNow; 
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
                Subject = new ClaimsIdentity(new Claim[] 
                {
                    //add new claims in here
                    new Claim(ClaimTypes.Name, user.Mail),
                    new Claim("name", user.Mail),
                    new Claim(ClaimTypes.Email, user.Mail),
                    new Claim(ClaimTypes.Role, "admin")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = _appSettings.Hostname,
                Issuer = _appSettings.Hostname,
            };

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