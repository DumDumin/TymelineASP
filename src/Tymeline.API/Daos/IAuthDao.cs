
using System.Collections.Generic;

public interface IAuthDao{
    IUser Register(IUser user);

    IEnumerable<string> GetUserPermissions(IUser user);

    Dictionary<int,IUser> GetUsers();

    IUser getUserById(int id);

    void RemoveUser(IUser user);

    IUser ChangePassword(IUser user, string password);
    

}