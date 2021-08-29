public interface IUser
{
    string UserId { get ; set; }
    string Email {get; set;}


    

    IUser verifyPassword(string passwd);
    DaoUser ToDaoUser();

}