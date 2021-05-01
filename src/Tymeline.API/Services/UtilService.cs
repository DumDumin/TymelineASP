using System.ComponentModel.DataAnnotations;

public class UtilService {
    public bool IsValidEmail(string source)
    {
        return new EmailAddressAttribute().IsValid(source);
    }
}