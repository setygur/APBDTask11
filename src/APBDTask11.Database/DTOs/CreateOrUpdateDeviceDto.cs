namespace APBDTask11.Database.DTOs;

public class CreateOrUpdateDeviceDto
{
    public string Name { get; set; }
    public string DeviceTypeName { get; set; }
    public bool IsEnabled { get; set; }
    public object AdditionalProperties { get; set; }
}