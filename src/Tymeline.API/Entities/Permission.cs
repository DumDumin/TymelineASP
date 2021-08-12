public class Role : IRole
{

    public Role(){}
    public Role(string type, string value){
        this.Type = type;
        this.Value = value;
    }
    public string Type { get ; set ; }
    public string Value { get ; set ; }

    public override bool Equals(object obj)
    {
        
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        
        return this.Type.Equals(((Role)obj).Type)&&this.Value.Equals(((Role)obj).Value);
    }

    public override int GetHashCode()
    {
        return this.Type.GetHashCode()+this.Value.GetHashCode();
    }
}
