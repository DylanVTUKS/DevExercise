namespace IOT_Devices.Models;

public class DeviceType
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }

    public List<Firmware> Firmware { get; set; } = new();
    public List<Device> Devices { get; set; } = new();
}