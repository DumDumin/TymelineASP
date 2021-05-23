using System.Collections.Generic;

public class AuthDao : IAuthDao
{
    public IUser ChangePassword(IUser user, string password)
    {
        throw new System.NotImplementedException();
    }

    public IUser getUserById(string id)
    {
        throw new System.NotImplementedException();
    }

    public IUser getUserById(int id)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<string> GetUserPermissions(IUser user)
    {
        throw new System.NotImplementedException();
    }

    public Dictionary<int,IUser> GetUsers()
    {
        throw new System.NotImplementedException();
    }



    public IUser Register(IUser user)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveUser(IUser user)
    {
        throw new System.NotImplementedException();
    }
}