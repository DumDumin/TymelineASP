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
        var claims = new Claim[]  
            {  
                        new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),  
                        new Claim(JwtRegisteredClaimNames.UniqueName, user.Mail),  
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),  
                        new Claim(JwtRegisteredClaimNames.Iat, utcNow.ToString()),  
                        new Claim("Admin","0")  
            };  
        var token = new JwtSecurityToken
        (
            signingCredentials : new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            claims : claims,
            audience : _appSettings.Hostname,
            issuer : _appSettings.Hostname,
            expires : DateTime.UtcNow.AddHours(1)
        );
        
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
                // var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

                // attach user to context on successful jwt validation
                
                return jwtToken;
            }
            catch
            {   
                return null;
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
            
    }
}