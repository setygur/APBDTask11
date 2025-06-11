namespace APBDTask11.WebApp.Helpers.CustomValidation.FileReaders;

using System.Text.Json;

public class JsonValidationReader : IValidationReader
{
    private readonly ILogger<JsonValidationReader> _logger;

    public JsonValidationReader(ILogger<JsonValidationReader> logger)
    {
        _logger = logger;
    }

    public List<ValidationRuleSet> LoadValidationRules()
    {
        _logger.LogInformation($"Loading validation rules at {DateTime.UtcNow}");
        var basePath = Directory.GetCurrentDirectory();
        var jsonPath = Path.Combine(basePath, "validation", "validation_rules.json");

        if (!File.Exists(jsonPath))
        {
            _logger.LogError($"No validation rules found at {jsonPath}");
            throw new FileNotFoundException($"Validation rules file not found at path: {jsonPath}");
        }

        var json = File.ReadAllText(jsonPath);

        var parsed = JsonSerializer.Deserialize<ValidationRuleFile>(json);

        _logger.LogInformation($"Loaded validation rules at {DateTime.UtcNow}");
        return parsed?.Validations ?? new List<ValidationRuleSet>();
    }
}