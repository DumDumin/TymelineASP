
using System.Collections.Generic;

public interface ITymelineObjectDao{
   TymelineObject getTymelineObjects();
   List<TymelineObject> getAll();

   TymelineObject getById(string id);

   List<TymelineObject> getByTime(int start,int end);

   void DeleteById(string id);

   TymelineObject Create(TymelineObject tymelineObject);

   TymelineObject UpdateById(string id, TymelineObject tymelineObject);

}