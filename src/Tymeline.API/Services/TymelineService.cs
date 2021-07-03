using System;
using System.Collections.Generic;
using Tymeline.API.Daos;

public class TymelineService : ITymelineService{

    ITymelineObjectDao _tymelineObjectDao;
    public TymelineService(ITymelineObjectDao tymelineObjectDao){
        _tymelineObjectDao = tymelineObjectDao;
    }
    public List<TymelineObject> GetAll(){
        return _tymelineObjectDao.getAll();
    }

    public TymelineObject GetById(string id){
        return _tymelineObjectDao.getById(id);
    }

    public Tuple<int,TymelineObject> Create(TymelineObject tymelineObject){
        throw new System.NotImplementedException();
    }
    public TymelineObject UpdateById(string id,TymelineObject tymelineObject){
        throw new System.NotImplementedException();
    }
    public void DeleteById(string id){
        _tymelineObjectDao.DeleteById(id);
    }

    public List<TymelineObject> GetByTime(int start, int end){
        return _tymelineObjectDao.getByTime(start,end);
    }



}