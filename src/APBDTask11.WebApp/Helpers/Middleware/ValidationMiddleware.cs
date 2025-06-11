using System.Text.Json;
using System.Text.RegularExpressions;
using APBDTask11.WebApp.Helpers.CustomValidation;
using APBDTask11.WebApp.Helpers.CustomValidation.FileReaders;

namespace APBDTask11.WebApp.Helpers.Middleware;

public class ValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly List<ValidationRuleSet> _rules;
    private readonly ILogger<ValidationMiddleware> _logger;

    public ValidationMiddleware(RequestDelegate next, IValidationReader validationReader,
        ILogger<ValidationMiddleware> logger)
    {
        _next = next;
        _rules = validationReader.LoadValidationRules();
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation($"Executing validation rules at {DateTime.UtcNow}");
        if (context.Request.Method != HttpMethods.Post && context.Request.Method != HttpMethods.Put)
        {
            _logger.LogInformation($"Request method is not POST or PUT");
            await _next(context);
            return;
        }

        context.Request.EnableBuffering();

        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        var jsonDoc = JsonDocument.Parse(body);
        var root = jsonDoc.RootElement;

        string type = root.GetProperty("type").GetString();
        var ruleSet = _rules.FirstOrDefault(r =>
            r.Type.Equals(type, StringComparison.OrdinalIgnoreCase) &&
            root.TryGetProperty(r.PreRequestName, out var prop) &&
            prop.GetString()?.Equals(r.PreRequestValue, StringComparison.OrdinalIgnoreCase) == true
        );

        if (ruleSet != null)
        {
            _logger.LogInformation($"Executing validation rules for DeviceType {type} at {DateTime.UtcNow}");
            foreach (var rule in ruleSet.Rules)
            {
                if (!root.TryGetProperty(rule.ParamName, out var paramValue)) continue;

                string value = paramValue.ToString();
                if (rule.RegexValue.ValueKind == JsonValueKind.String)
                {
                    var pattern = rule.RegexValue.GetString();
                    if (!Regex.IsMatch(value, pattern))
                    {
                        _logger.LogWarning($"Validation rule {rule.ParamName}," +
                                               $" for value: {value}, pattern {pattern} is invalid");
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync($"Validation failed for {rule.ParamName}:" +
                                                          $" does not match pattern.");
                        return;
                    }
                }
            }
            _logger.LogInformation($"Executing validation successful for DeviceType {type} at {DateTime.UtcNow}");
        }
        else
        {
            _logger.LogInformation($"No extra validation for DeviceType {type} found, moving on at {DateTime.UtcNow}");
        }

        await _next(context);
    }
}