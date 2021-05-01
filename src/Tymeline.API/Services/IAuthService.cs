using Microsoft.IdentityModel.Tokens;

public interface IAuthService
{

    SigningCredentials GetSigningCredentials();

    IUser Login(UserCredentials credentials);
    IUser Register(UserRegisterCredentials credentials);
}