using System.Collections.Generic;

public class TymelineObjectRoles : ITymelineObjectRoles
{
    public List<IRole> Roles {get;set;}
    public TymelineObject TymelineObject {get;set;}
}