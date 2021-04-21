public class Content{
    public string Text { get; set; }

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

    public TymelineObject(string identifier, int lenghtTime, Content contents,int startTime,bool cChangeLength,bool cMove){
        Id=identifier;
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
    
    public override int GetHashCode()
    {
        return this.Id.GetHashCode();
    }
}