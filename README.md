Roles vs Claims


Add roles via

```
new Claim(ClaimTypes.Role, {{rolename}})
```

in the jwt service. Populate them through the IUser interface ( or maybe thought some SecuretyUser)

add claims via

```
new Claim(ClaimTypes.{{ClaimName}}, {{value}})
```

you can ask for roles in the "Authentication" decorator
for claims you need to create some policy based on the existance or lack of Claim
https://docs.microsoft.com/en-us/aspnet/core/security/authorization/claims?view=aspnetcore-5.0

ALL OF THIS IS UNCESSARY TO USING THIS BACKEND AS STORAGE FOR TYMELINE DATA AND ACCOUNT INFORMATION
ALL OF THIS IS NECCESARY ONLY FOR INTEGRATING A COMPREHENSIVE ACCOUNT MANAGEMENT STRUCTURE

Thinking about how to delegate rights:
assign and revoke
who can create Data?
- any account
who can assign Data to Roles?
- any User in Role
- any User in Supervisory Role
who can assign Roles to Users?
- any User in Supervisory Role
who can access Roles for users?
- the specific User
who can access Data for Roles?
- any user with these Roles
who can access Data for Users?
- the specific User
who can delete Data?
- Any user who is member of all Roles with access to the Item
- if a user can remove data for different Roles, that is unexpected
who can delete Roles?
- Any User who is in supervisory role to all of these Roles
who can delete Users?
- User in Supervisory Role to that User


define Supervisory Role
- different rights structure needed for managing inter-User rights management
- map Users to User
- can be mapped recursively
- user cannot map onto themselves ( could set rights for themselves)
- admin user is created at the onset, has all Users mapped to them
- admin user rights tbd

- 

```
``` 