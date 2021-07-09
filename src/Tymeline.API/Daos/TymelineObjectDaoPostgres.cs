using System.Collections.Generic;
using System.Transactions;

namespace Tymeline.API.Daos
{
    public class TymelineObjectDaoPostgres: ITymelineObjectDao{

        

        public TymelineObjectDaoPostgres(){
            
        }
        public TymelineObject getTymelineObjects(){
            throw new System.NotImplementedException();
        }    

        public List<TymelineObject> getAll(){
            using (TransactionScope ts = new TransactionScope())
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

        public TymelineObject Create(TymelineObject tymelineObject)
        {
            throw new System.NotImplementedException();
        }

        public TymelineObject UpdateById(string id, TymelineObject tymelineObject)
        {
            throw new System.NotImplementedException();
        }
    }
}