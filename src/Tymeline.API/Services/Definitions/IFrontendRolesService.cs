public interface IFrontendRolesService{

    IUserPermissions GetUserPermissions(string email);

    void AddRole(string roleName);

    void RemoveRole(string roleName);

    void SetUserPermissions(IUserPermissions permissions);

    void AddUserPermissions(string email, IPermission permission);

    void RemoveUserPermissions(string email, IPermission permission);
}