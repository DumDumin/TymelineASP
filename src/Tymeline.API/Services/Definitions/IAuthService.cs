using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;

public interface IAuthService
{


    IUser Login(IUserCredentials credentials);
    IUser Register(IUserCredentials credentials);
    List<IUser> getUsers();

    void RemoveUser(IUser user);
    IUser ChangePassword(IUser user, string passwd);
    IUser GetByMail(string mail);

    IUserRoles GetUserRoles(string email);

    void SetUserRoles(IUserRoles userPermissions);

    void AddUserRole(IUserRole userPermission);

    void RemoveUserRole(IUserRole userRole);
    

}