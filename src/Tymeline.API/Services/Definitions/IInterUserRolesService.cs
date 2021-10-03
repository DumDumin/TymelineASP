public interface IInterUserRolesService
{
    IUserRoles GetUserRoles(string email);
    bool userHasPermissionOnUser(string Email, string ProbeEmail, Roles minPermission);
    bool userHasPermissionOnUser(string Email, IUser ProbeUser, Roles minPermission);
    bool userHasPermissionOnUser(IUser User, IUser ProbeUser, Roles minPermission);
    bool userHasPermissionOnUser(IUser User, string ProbeEmail, Roles minPermission);
}