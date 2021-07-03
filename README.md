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