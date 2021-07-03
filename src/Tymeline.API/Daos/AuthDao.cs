using System.Collections.Generic;

public class AuthDao : IAuthDao
{
    public IUser ChangePassword(IUser user, string password)
    {
        throw new System.NotImplementedException();
    }



    public IUser getUserByMail(string email)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<string> GetUserPermissions(IUser user)
    {
        throw new System.NotImplementedException();
    }

    public Dictionary<string,IUser> GetUsers()
    {
        throw new System.NotImplementedException();
    }



    public IUser Register(IUserCredentials user)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveUser(IUser user)
    {
        throw new System.NotImplementedException();
    }
}