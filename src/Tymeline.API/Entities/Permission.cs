public class Permission : IPermission
{

    public Permission(){}
    public Permission(string key, string value){
        this.Key = key;
        this.Value = value;
    }
    public string Key { get ; set ; }
    public string Value { get ; set ; }

    public override bool Equals(object obj)
    {
        
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        
        return this.Key.Equals(((Permission)obj).Key)&&this.Value.Equals(((Permission)obj).Value);
    }
}
