public class Permission : IPermission
{

    public Permission(){}
    public Permission(string type, string value){
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
        
        return this.Type.Equals(((Permission)obj).Type)&&this.Value.Equals(((Permission)obj).Value);
    }

    public override int GetHashCode()
    {
        return this.Type.GetHashCode()+this.Value.GetHashCode();
    }
}
