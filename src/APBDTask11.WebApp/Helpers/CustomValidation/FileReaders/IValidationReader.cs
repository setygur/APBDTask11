namespace APBDTask11.WebApp.Helpers.CustomValidation.FileReaders;

public interface IValidationReader
{
    List<ValidationRuleSet> LoadValidationRules();
}