using System.Text.Json;
using System.Text.Json.Serialization;

namespace APBDTask11.WebApp.Helpers.CustomValidation;

public class ValidationRule
{
    public string ParamName { get; set; }
    
    [JsonPropertyName("regex")]
    public JsonElement RegexValue { get; set; }
}