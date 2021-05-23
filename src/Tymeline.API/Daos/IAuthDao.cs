
using System.Collections.Generic;

public interface IAuthDao{
    bool register(UserRegisterCredentials credentials);

    IUser Login(UserCredentials credentials);


}