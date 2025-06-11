namespace APBDTask11.WebApp.Helpers.CustomValidation;

public class ValidationRuleSet
{
    public string Type { get; set; }
    public string PreRequestName { get; set; }
    public string PreRequestValue { get; set; }
    public List<ValidationRule> Rules { get; set; }
}