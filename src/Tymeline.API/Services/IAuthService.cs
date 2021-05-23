using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;

public interface IAuthService
{


    IUser Login(IUserCredentials credentials);
    IUser Register(IUserCredentials credentials);
    List<IUser> getUsers();

    void RemoveUser(IUser user);
    IUser ChangePassword(IUser user, string passwd);
    IUser GetById(int id);
    // string createPassword(User BaseUser);
    // bool verifyPassword(string Password,User BaseUser);
    string CreateJWT(IUser user);
    bool verifyJWT(string jwt, IUser user);
}