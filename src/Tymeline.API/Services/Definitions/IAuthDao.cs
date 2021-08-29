
using System.Collections.Generic;

public interface IAuthDao{
    IUser Register(IUserCredentials credentials);
    
    // IEnumerable<string> GetUserPermissions(IUser user);

    List<IUser> GetUsers();

    IUser getUserByMail(string mail);

    void RemoveUser(IUser user);

    IUser ChangePassword(IUser user, string password);


    

}