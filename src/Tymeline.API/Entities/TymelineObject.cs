using System;

public class Content{
    public string Text { get; set; }

    public Content(){
        
    }

    public Content(string content){
        Text = content;
    }
}


public class TymelineObject{
    public string Id { get; set; }
    public int Length { get; set; }
    public Content Content { get; set; }
    public int Start { get; set; }
    public bool CanChangeLength { get; set; }
    public bool CanMove { get; set; }

    public TymelineObject(){

    }

    public TymelineObject(string identifier, int lenghtTime, Content contents,int startTime,bool cChangeLength,bool cMove){
        Id=identifier;
        Length = lenghtTime;
        Content = contents;
        Start = startTime;
        CanChangeLength = cChangeLength;
        CanMove =cMove;
    }

     public TymelineObject( int lenghtTime, Content contents,int startTime,bool cChangeLength,bool cMove){
        Id = Guid.NewGuid().ToString();
        Length = lenghtTime;
        Content = contents;
        Start = startTime;
        CanChangeLength = cChangeLength;
        CanMove =cMove;
    }

    public override bool Equals(object obj)
    {
        
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        return this.Id.Equals(((TymelineObject)obj).Id);
    }

    public bool Same(TymelineObject tymelineObject){
        return this.GetHashCode().Equals(tymelineObject.GetHashCode());
    }

    public override int GetHashCode(){
        return this.Id.GetHashCode()+this.Length.GetHashCode()+this.Content.GetHashCode()+this.Start.GetHashCode();
    }
    
}