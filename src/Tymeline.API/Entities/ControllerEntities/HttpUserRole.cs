public class HttpUserRole{

    public HttpUserRole(string email, Role role){
        this.Email = email;
        this.Role = role;
    }


    public IUserRole ToIUserRole(){
        var s = new UserRole(this.Email, this.Role);
        return s;
    }
    public string Email{get;set;}
    public Role Role{get;set;} 
}