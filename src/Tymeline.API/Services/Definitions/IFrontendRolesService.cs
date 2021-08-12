public interface IFrontendRolesService{

    IUserRoles GetUserPermissions(string email);

    void AddRole(string roleName);

    void RemoveRole(string roleName);

    void SetUserPermissions(IUserRoles permissions);

    void AddUserPermissions(string email, IRole permission);

    void RemoveUserPermissions(string email, IRole permission);
}