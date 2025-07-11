namespace APBDTask11.Database.Models;

public class Device
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsEnabled { get; set; }
    public string AdditionalProperties { get; set; }
    public int? DeviceTypeId { get; set; }

    public DeviceType DeviceType { get; set; }
    public ICollection<DeviceEmployee> DeviceEmployees { get; set; }
}