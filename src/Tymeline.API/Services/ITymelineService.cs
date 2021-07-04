using System;
using System.Collections.Generic;

public interface ITymelineService{
    List<TymelineObject> GetAll();



    TymelineObject GetById(int id);

    List<TymelineObject> GetByTime(int start,int end);
    TymelineObject Create(TymelineObject tymelineObject);
    TymelineObject UpdateById(int id,TymelineObject tymelineObject);
    void DeleteById(int id);
}