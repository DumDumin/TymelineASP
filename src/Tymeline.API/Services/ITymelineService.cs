using System.Collections.Generic;

public interface ITymelineService{
    List<TymelineObject> getAll();
    TymelineObject getById(string id);
    TymelineObject create(string JsonPayload);
    TymelineObject updateById(string id,string JsonPayload);
    TymelineObject deleteById(string id);
}