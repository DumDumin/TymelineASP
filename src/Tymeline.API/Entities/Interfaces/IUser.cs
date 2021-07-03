public interface IUser
{
    int UserId { get ; set; }
    string Mail {get; set;}


    IUser updatePassword(string password);

    IUser verifyPassword(string passwd);


}