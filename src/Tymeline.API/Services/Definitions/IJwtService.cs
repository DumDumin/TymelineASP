using System.IdentityModel.Tokens.Jwt;

public interface IJwtService{

    string createJwt(string userMail);
    JwtSecurityToken verifyJwt(string jwt);
}