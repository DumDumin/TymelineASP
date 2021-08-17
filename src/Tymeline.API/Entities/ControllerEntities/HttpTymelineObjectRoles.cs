using System.Collections.Generic;

public record HttpTymelineObjectRoles{

    public TymelineObject tymelineObject{get;set;}
    public List<Role> Roles{get;set;}
}

public record HttpTymelineObjectRolesIncrement{

    public TymelineObject tymelineObject{get;set;}
    public Role Role{get;set;}
}


