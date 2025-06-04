namespace APBDTask11.Database.DTOs;

public class DeviceDetailsDto
{
    public string Name { get; set; }
    public string DeviceTypeName { get; set; }
    public bool IsEnabled { get; set; }
    public object AdditionalProperties { get; set; }
    public DeviceEmployeeDto? CurrentEmployee { get; set; }
}