
using System.Collections.Generic;

public interface ITymelineObjectDao{
   TymelineObject getTymelineObjects();
   List<TymelineObject> getAll();

   TymelineObject getById(string id);

   List<TymelineObject> getByStart(int after,int before);
   List<TymelineObject> getByEnd(int after,int before);


}