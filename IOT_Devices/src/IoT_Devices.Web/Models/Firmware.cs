using System.ComponentModel.DataAnnotations;
namespace IOT_Devices.Models;

public class Firmware
{
    public int Id { get; set; }
    public int DeviceTypeId { get; set; }
    [Required, StringLength(50)]
    public string Version { get; set; } = "";
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly ReleaseDate { get; set; }
    [StringLength(500)]
    public string? Notes { get; set; }

    public DeviceType? DeviceType { get; set; }
    public List<Device> Devices { get; set; } = new();
}