using System.Collections.Generic;

namespace Tymeline.API.Daos
{
    public class TymelineObjectDao: ITymelineObjectDao{
        public TymelineObject getTymelineObjects(){
        return null;
        }    

        public List<TymelineObject> getAll(){
            return null;
        }

        public TymelineObject getById(string id)
        {
            throw new System.NotImplementedException();
        }

        public List<TymelineObject> getByStart(int after, int before)
        {
            throw new System.NotImplementedException();
        }

        public List<TymelineObject> getByEnd(int after, int before)
        {
            throw new System.NotImplementedException();
        }
    }
}