using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;

public interface IAuthService
{

    SigningCredentials GetSigningCredentials();

    IUser Login(UserCredentials credentials);
    IUser Register(UserRegisterCredentials credentials);
    List<IUser> getUsers();

    IUser GetById(int id);
    // string createPassword(User BaseUser);
    // bool verifyPassword(string Password,User BaseUser);
    string CreateJWT(IUser user);
    bool verifyJWT(string jwt, IUser user);
}