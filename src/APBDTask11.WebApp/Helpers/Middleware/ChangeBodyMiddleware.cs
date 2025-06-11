using System.Text;
using System.Text.Json;

namespace APBDTask11.WebApp.Helpers.Middleware;

public class ChangeBodyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ChangeBodyMiddleware> _logger;

    public ChangeBodyMiddleware(RequestDelegate next, ILogger<ChangeBodyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation($"Executing ChangeBody at {DateTime.UtcNow}");
        if (context.Request.ContentType?.Contains("application/json") == true &&
            (context.Request.Method == HttpMethods.Post || context.Request.Method == HttpMethods.Put))
        {
            _logger.LogInformation($"ChangeBody found post/put json payload at {DateTime.UtcNow}");
            context.Request.EnableBuffering();

            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (!string.IsNullOrWhiteSpace(body))
            {
                var transformed = TransformRequestBody(context, body);
                var bytes = Encoding.UTF8.GetBytes(transformed);

                context.Request.Body = new MemoryStream(bytes);
                context.Request.ContentLength = bytes.Length;
            }

            _logger.LogInformation($"ChangeBody successfully modified post/put json payload at {DateTime.UtcNow}");
        }

        _logger.LogInformation($"ChangeBody started modifying response at {DateTime.UtcNow}");
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

        var modifiedResponse = TransformResponseBody(context, responseBody);

        var responseBytes = Encoding.UTF8.GetBytes(modifiedResponse);
        await context.Response.Body.WriteAsync(responseBytes);
        _logger.LogInformation($"ChangeBody successfully modified response at {DateTime.UtcNow}");
        await _next(context);
    }

    private string TransformRequestBody(HttpContext context, string body)
    {
        _logger.LogInformation($"Transforming request body at {DateTime.UtcNow}");
        var path = context.Request.Path.Value?.ToLower();
        var method = context.Request.Method;

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        if (context.Request.Method == HttpMethods.Post && path == "/api/auth")
        {
            _logger.LogInformation($"Found HTTP method POST  at \"/api/auth\" at {DateTime.UtcNow}");
            var response = new
            {
                username = root.GetProperty("username").GetString(),
                token = root.TryGetProperty("token", out var tok) ? tok.GetString() : null
            };

            _logger.LogInformation($"Transformed request body for POST at \"/api/auth\" at {DateTime.UtcNow}");
            return JsonSerializer.Serialize(response);
        }
        
        if (method == HttpMethods.Post && path == "/api/devices")
        {
            _logger.LogInformation($"Found HTTP method POST  at \"/api/devices\" at {DateTime.UtcNow}");
            if (root.TryGetProperty("type", out var type))
            {
                string name = root.GetProperty("name").GetString();
                bool isEnabled = root.GetProperty("isEnabled").GetBoolean();
                
                var knownKeys = new[] { "name", "isEnabled", "type" };
                var additionalProps = new Dictionary<string, JsonElement>();

                foreach (var prop in root.EnumerateObject())
                {
                    if (!knownKeys.Contains(prop.Name))
                    {
                        additionalProps[prop.Name] = prop.Value.Clone();
                    }
                }

                var payload = new
                {
                    name,
                    isEnabled,
                    additionalProperties = additionalProps,
                    type = type.GetString()
                };
                
                _logger.LogInformation($"Transformed request body for POST at \"/api/devices\" at {DateTime.UtcNow}");
                return JsonSerializer.Serialize(payload);
            }
        }
        if (method == HttpMethods.Post && path == "/api/employees")
        {
            _logger.LogInformation($"Found HTTP method POST  at \"/api/employees\" at {DateTime.UtcNow}");
            if (root.TryGetProperty("salary", out var salary))
            {
                var knownKeys = new[] { "salary", "positionId" };
                var person = new Dictionary<string, JsonElement>();

                foreach (var prop in root.EnumerateObject())
                {
                    if (!knownKeys.Contains(prop.Name))
                    {
                        person[prop.Name] = prop.Value.Clone();
                    }
                }

                var payload = new
                {
                    person,
                    salary = salary.GetDouble(),
                    positionId = root.GetProperty("positionId").GetInt32()
                };
                
                _logger.LogInformation($"Transformed request body for POST at \"/api/employees\" at {DateTime.UtcNow}");
                return JsonSerializer.Serialize(payload);
            }
        }
        if (method == HttpMethods.Post && path == "/api/accounts")
        {
            _logger.LogInformation($"Found HTTP method POST  at \"/api/accounts\" at {DateTime.UtcNow}");
            if (root.TryGetProperty("username", out var username))
            {
                
                var payload = new
                {
                    username = username.GetString(),
                    password = root.GetProperty("password").GetString(),
                    employeeId = root.GetProperty("employeeId").GetInt32(),
                    roleId = root.GetProperty("roleId").GetInt32()
                };
                
                _logger.LogInformation($"Transformed request body for POST at \"/api/accounts\" at {DateTime.UtcNow}");
                return JsonSerializer.Serialize(payload);
            }
        }

        _logger.LogInformation($"$Request body was not json or empty returning original at {DateTime.UtcNow}");
        return body;
    }

    private string TransformResponseBody(HttpContext context, string body)
    {
        _logger.LogInformation($"Transforming response body at {DateTime.UtcNow}");
        var path = context.Request.Path.Value?.ToLower();

        if (context.Response.ContentType?.Contains("application/json") != true || string.IsNullOrWhiteSpace(body))
        {
            _logger.LogInformation($"Response body was not json or empty at {DateTime.UtcNow}");
            return body;
        }

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        
        if (context.Request.Method == HttpMethods.Get && path != null && path.StartsWith("/api/devices/"))
        {
            _logger.LogInformation($"Found HttpMethod GET at \"/api/devices/\" at {DateTime.UtcNow}");
            if (root.TryGetProperty("type", out var type))
            {
                string name = root.GetProperty("name").GetString();
                bool isEnabled = root.GetProperty("isEnabled").GetBoolean();
                
                var knownKeys = new[] { "name", "isEnabled", "type" };
                var additionalProps = new Dictionary<string, JsonElement>();

                foreach (var prop in root.EnumerateObject())
                {
                    if (!knownKeys.Contains(prop.Name))
                    {
                        additionalProps[prop.Name] = prop.Value.Clone();
                    }
                }

                var payload = new
                {
                    name = root.GetProperty("name").GetString(),
                    isEnabled = root.GetProperty("isEnabled").GetBoolean(),
                    additionalProperties = additionalProps,
                    type = type.GetString()
                };
                
                _logger.LogInformation($"Transformed response body for POST at \"/api/devices\" at {DateTime.UtcNow}");
                return JsonSerializer.Serialize(payload);
            }
        }

        if (context.Request.Method == HttpMethods.Get && path != null && path.StartsWith("/api/accounts/"))
        {
            _logger.LogInformation($"Found HttpMethod GET at \"/api/accounts/\" at {DateTime.UtcNow}");
            if (root.TryGetProperty("username", out var username))
            {
                var payload = new
                {
                    username = username.GetString(),
                    roleName = root.GetProperty("roleName")
                };
                
                _logger.LogInformation($"Transformed response body for POST at \"/api/accounts/\" at {DateTime.UtcNow}");
                return JsonSerializer.Serialize(payload);
            }
        }
        
        if (context.Request.Method == HttpMethods.Get && path != null && path.StartsWith("/api/employees/"))
        {
            _logger.LogInformation($"Found HttpMethod GET at \"/api/employees/\" at {DateTime.UtcNow}");
            if (root.TryGetProperty("salary", out var salary))
            {
                
                var knownKeys = new[] { "salary", "position", "hireDate" };
                var person = new Dictionary<string, JsonElement>();

                foreach (var prop in root.EnumerateObject())
                {
                    if (!knownKeys.Contains(prop.Name))
                    {
                        person[prop.Name] = prop.Value.Clone();
                    }
                }
                
                var payload = new
                { 
                    person,
                    salary = salary.GetDouble(),
                    position = root.TryGetProperty("position", out var position) ? position.GetString() : null,
                    hireDate = root.TryGetProperty("hireDate", out var hireDate) ? hireDate.GetString() : null
                };
                
                _logger.LogInformation($"Transformed response body for POST at \"/api/employees/\" at {DateTime.UtcNow}");
                return JsonSerializer.Serialize(payload);
            }
        }
        
        if (context.Request.Method == HttpMethods.Get && (path == "/api/devices" ||
                                                          path == "/api/roles" || path == "/api/positions" ||
                                                          path == "api/devices/types"))
        {
            var allowedKeys = new[] { "id", "name" };
            _logger.LogInformation($"Returning list of devices/roles/positions/types at {DateTime.UtcNow}");
            return EnforceAllowedFieldsInListResponse(doc, allowedKeys);
        }
        if (context.Request.Method == HttpMethods.Get && path == "/api/employees")
        {
            var allowedKeys = new[] { "id", "fullName" };
            _logger.LogInformation($"Returning list of employees at {DateTime.UtcNow}");
            return EnforceAllowedFieldsInListResponse(doc, allowedKeys);
        }
        _logger.LogInformation($"Response body was not json or empty returning original at {DateTime.UtcNow}");
        return body;
    }
    
    private string EnforceAllowedFieldsInListResponse(JsonDocument doc, string[] allowedKeys)
    {
        _logger.LogInformation($"Enforcing allowed fields in list response at {DateTime.UtcNow}");
        var sanitizedList = new List<Dictionary<string, object>>();
        foreach (var item in doc.RootElement.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
                continue;
            var sanitizedItem = new Dictionary<string, object>();
            foreach (var key in allowedKeys)
            {
                if (item.TryGetProperty(key, out var prop))
                    sanitizedItem[key] = JsonElementToObject(prop);
            }
            sanitizedList.Add(sanitizedItem);
        }
        _logger.LogInformation($"Enforced allowed fields in list response at {DateTime.UtcNow}");
        return JsonSerializer.Serialize(sanitizedList);
    }
    
    private object? JsonElementToObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Object => JsonSerializer.Deserialize<Dictionary<string, object>>(element.GetRawText()),
            JsonValueKind.Array => JsonSerializer.Deserialize<List<object>>(element.GetRawText()),
            _ => element.GetRawText()
        };
    }

}
