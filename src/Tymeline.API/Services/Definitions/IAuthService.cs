using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;

public interface IAuthService
{

    IUserRoles GetUserRoles(string email);
    IUser Login(IUserCredentials credentials);
    IUser Register(IUserCredentials credentials);
    List<IUser> getUsers();

    void RemoveUser(IUser user);
    IUser ChangePassword(IUser user, string passwd);
    IUser GetByMail(string mail);

}