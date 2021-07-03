using System;
using System.Collections.Generic;

public interface ITymelineService{
    List<TymelineObject> GetAll();



    TymelineObject GetById(string id);

    List<TymelineObject> GetByTime(int start,int end);
    Tuple<int,TymelineObject> Create(TymelineObject tymelineObject);
    TymelineObject UpdateById(string id,TymelineObject tymelineObject);
    void DeleteById(string id);
}