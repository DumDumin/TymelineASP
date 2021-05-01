using System.Collections.Generic;
using Tymeline.API.Daos;

public class TymelineService : ITymelineService{

    ITymelineObjectDao _tymelineObjectDao;
    public TymelineService(ITymelineObjectDao tymelineObjectDao){
        _tymelineObjectDao = tymelineObjectDao;
    }
    public List<TymelineObject> getAll(){
        return _tymelineObjectDao.getAll();
    }

    public TymelineObject getById(string id){
        return null;
    }

    public TymelineObject create(string JsonPayload){
        return null;
    }
    public TymelineObject updateById(string id,string JsonPayload){
        return null;
    }
    public TymelineObject deleteById(string id){
        return null;
    }



}