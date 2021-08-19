using System.Collections.Generic;

public class TymelineObjectRoles : ITymelineObjectRoles
{

    public TymelineObjectRoles(){}


    public TymelineObjectRoles(TymelineObject tymelineObject, List<IRole> roles){
        this.TymelineObject = tymelineObject;
        this.Roles = roles;
    }
    public List<IRole> Roles {get;set;}
    public TymelineObject TymelineObject {get;set;}
}