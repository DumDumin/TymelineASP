using System.Collections.Generic;

namespace Tymeline.API.Daos
{
    public class TymelineObjectDao: ITymelineObjectDao{
        public TymelineObject getTymelineObjects(){
            throw new System.NotImplementedException();
        }    

        public List<TymelineObject> getAll(){
            throw new System.NotImplementedException();
        }

        public TymelineObject getById(string id)
        {
            throw new System.NotImplementedException();
        }

        public List<TymelineObject> getByTime(int start, int end)
        {
            throw new System.NotImplementedException();
        }


        public void DeleteById(string id){
            throw new System.NotImplementedException();
        }

    }
}