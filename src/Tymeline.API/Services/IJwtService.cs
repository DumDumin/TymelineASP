public interface IJwtService{

    string createJwt(IUser user);
    IUser verifyJwt(string jwt);
}