using System.Collections.Generic;

public interface ITymelineObjectRoles{
    List<IRole> Roles{get; set;}
    TymelineObject TymelineObject{get; set;}

}