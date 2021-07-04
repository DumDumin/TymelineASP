public interface IUser
{
    string UserId { get ; set; }
    string Mail {get; set;}


    IUser updatePassword(string password);

    IUser verifyPassword(string passwd);


}