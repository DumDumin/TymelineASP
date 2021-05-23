public interface IUser
{
    int UserId { get ; set; }
    string Mail {get; set;}

    public bool verifyPassword(string passwd);


}