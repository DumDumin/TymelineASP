using System.IdentityModel.Tokens.Jwt;

public interface IJwtService{

    string createJwt(IUser user);
    JwtSecurityToken verifyJwt(string jwt);
}