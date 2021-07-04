
using System.Collections.Generic;

public interface ITymelineObjectDao{
   TymelineObject getTymelineObjects();
   List<TymelineObject> getAll();

   TymelineObject getById(int id);

   List<TymelineObject> getByTime(int start,int end);

   void DeleteById(int id);

   TymelineObject Create(TymelineObject tymelineObject);

   TymelineObject UpdateById(int id, TymelineObject tymelineObject);

}