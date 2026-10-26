using System.ComponentModel.DataAnnotations;
namespace IOT_Devices.Models;

public class Device
{
    public int Id { get; set; }
    [Required, StringLength(50)]
    public string SerialNumber { get; set; } = "";

    [Required, StringLength(100)]
    public string Name { get; set; } = "";
    public int DeviceTypeId { get; set; }
    public int FirmwareId { get; set; }
    public int? GroupId { get; set; }
    public DateTime CreatedAt { get; set; }

    public DeviceType? DeviceType { get; set; }
    public Firmware? Firmware { get; set; }
    public DeviceGroup? Group { get; set; }
}