using System;
using System.Collections.Generic;

public interface ITymelineService
{
    List<TymelineObject> GetAllForUser(string UserId, Roles minRole);
    TymelineObject GetById(string id);
    List<TymelineObject> GetByTime(int start, int end);
    TymelineObject Create(TymelineObject tymelineObject);
    TymelineObject UpdateById(string id, TymelineObject tymelineObject);
    void DeleteById(string id);
}