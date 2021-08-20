using System.Collections.Generic;

public record HttpTymelineObjectRoles{

    public string tymelineObjectId{get;set;}
    public List<Role> Roles{get;set;}
}

public record HttpTymelineObjectRolesIncrement{

    public string tymelineObjectId{get;set;}
    public Role Role{get;set;}
}


