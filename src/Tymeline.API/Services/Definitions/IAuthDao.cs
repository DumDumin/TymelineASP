
using System.Collections.Generic;

public interface IAuthDao{
    IUser Register(IUserCredentials user);

    IEnumerable<string> GetUserPermissions(IUser user);

    Dictionary<string,IUser> GetUsers();

    IUser getUserByMail(string mail);

    void RemoveUser(IUser user);

    IUser ChangePassword(IUser user, string password);
    

}