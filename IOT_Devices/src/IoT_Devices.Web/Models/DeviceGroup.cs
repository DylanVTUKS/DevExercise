using System.ComponentModel.DataAnnotations;
namespace IOT_Devices.Models;

public class DeviceGroup
{
    public int Id { get; set; }
    [Required, StringLength(100)]
    public string Name { get; set; } = "";
    public int? ParentGroupId { get; set; }

    public DeviceGroup? Parent { get; set; }
    public List<DeviceGroup> Children { get; set; } = new();
    public List<Device> Devices { get; set; } = new();
    public int TotalDeviceCount => Devices.Count + Children.Sum(c => c.TotalDeviceCount); //Used to count child-group devices
}