public interface IUser
{
    string UserId { get ; set; }
    string Email {get; set;}


    IUser updatePassword(string password);

    IUser verifyPassword(string passwd);


}