using Microsoft.IdentityModel.Tokens;

class AuthService : IAuthService
{
    public SigningCredentials GetSigningCredentials()
    {
        throw new System.NotImplementedException();
    }

    public IUser Login(UserCredentials credentials)
    {
        throw new System.NotImplementedException();
    }

    public IUser Register(UserRegisterCredentials credentials)
    {
        throw new System.NotImplementedException();
    }
}
