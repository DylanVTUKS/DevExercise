using IOT_Devices.Data;
using IOT_Devices.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IOT_Devices.Controllers;

public class DevicesController : Controller
{
    private readonly AppDbContext _db;

    public DevicesController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /Devices
    public async Task<IActionResult> Index()
    {
        var devices = await _db.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.Firmware)
            .OrderBy(d => d.Name)
            .ToListAsync();

        ViewBag.GroupPaths = await GetGroupPaths();
        return View(devices);
    }

    // GET: /Devices/Create
    public async Task<IActionResult> Create()
    {
        await LoadCreateOptions();
        return View(new Device {});
    }

    // POST: /Devices/Create
    [HttpPost]
    [ValidateAntiForgeryToken] //Security check
    public async Task<IActionResult> Create(
        [Bind("SerialNumber,Name,DeviceTypeId,FirmwareId,GroupId,IsActive")] Device device)
    {
        device.SerialNumber = (device.SerialNumber ?? "").Trim();
        device.Name = (device.Name ?? "").Trim();

        if (!await _db.DeviceTypes.AnyAsync(t => t.Id == device.DeviceTypeId))
        {
            ModelState.AddModelError("DeviceTypeId", "Choose a device type. ");
        }

        var firmware = await _db.Firmware.FindAsync(device.FirmwareId);
        if (firmware == null)
        {
            ModelState.AddModelError("FirmwareId", "Choose a firmware version");
        }
        else if (firmware.DeviceTypeId != device.DeviceTypeId)
        {
            ModelState.AddModelError("FirmwareId", "Firmware not for this device. ");
        }

        if (device.GroupId != null && !await _db.DeviceGroups.AnyAsync(g => g.Id == device.GroupId))
        {
            ModelState.AddModelError("GroupId", "Group does not exist.");
        }

        if (await _db.Devices.AnyAsync(d => d.SerialNumber == device.SerialNumber))
        {
            ModelState.AddModelError("SerialNumber", "Serial number already used. ");
        }

        if (!ModelState.IsValid)
        {
            await LoadCreateOptions(device.DeviceTypeId, device.GroupId);
            return View(device);
        }

        device.CreatedAt = DateTime.UtcNow;
        _db.Devices.Add(device);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Devices/Edit/ID
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var device = await _db.Devices.Include(d => d.DeviceType)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (device == null) return NotFound();

        await LoadEditOptions(device);
        return View(device);
    }

    // POST: /Devices/Edit/ID
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id, [Bind("Id,Name,FirmwareId,GroupId,IsActive")] Device input)
    {
        if (id != input.Id) return NotFound();
        var device = await _db.Devices.Include(d => d.DeviceType)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (device == null) return NotFound();
        ModelState.Remove(nameof(Device.SerialNumber));

        input.Name = (input.Name ?? "").Trim();

        var firmware = await _db.Firmware.FindAsync(input.FirmwareId);
        if (firmware == null || firmware.DeviceTypeId != device.DeviceTypeId)
        {
            ModelState.AddModelError("FirmwareId", "Choose a firmware for this device. ");
        }

        if (input.GroupId != null && !await _db.DeviceGroups.AnyAsync(g => g.Id == input.GroupId))
        {
            ModelState.AddModelError("GroupId", "Group does not exist. ");
        }

        if (!ModelState.IsValid)
        {
            input.SerialNumber = device.SerialNumber;
            input.DeviceTypeId = device.DeviceTypeId;
            input.DeviceType = device.DeviceType;
            await LoadEditOptions(input);
            return View(input);
        }

        device.Name = input.Name;
        device.FirmwareId = input.FirmwareId;
        device.GroupId = input.GroupId;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    private async Task LoadCreateOptions(int? selectedTypeId = null, int? selectedGroupId = null)
    {
        var types = await _db.DeviceTypes.OrderBy(t => t.Name).ToListAsync();
        ViewBag.TypeOptions = new SelectList(types, "Id", "Name", selectedTypeId);

        ViewBag.FirmwareList = await _db.Firmware
            .OrderBy(f => f.DeviceTypeId).ThenByDescending(f => f.ReleaseDate)
            .ToListAsync();

        await LoadGroupOptions(selectedGroupId);
    }

    private async Task LoadEditOptions(Device device)
    {
        var firmware = await _db.Firmware
            .Where(f => f.DeviceTypeId == device.DeviceTypeId)
            .OrderByDescending(f => f.ReleaseDate)
            .ToListAsync();
        ViewBag.FirmwareOptions = new SelectList(firmware, "Id", "Version", device.FirmwareId);

        await LoadGroupOptions(device.GroupId);
    }

    private async Task LoadGroupOptions(int? selectedId)
    {
        var paths = await GetGroupPaths();
        var items = paths.OrderBy(p => p.Value)
            .Select(p => new { Id = p.Key, Name = p.Value });
        ViewBag.GroupOptions = new SelectList(items, "Id", "Name", selectedId);
    }

    private async Task<Dictionary<int, string>> GetGroupPaths()
    {
        var groups = await _db.DeviceGroups.AsNoTracking().ToListAsync();
        var byId = groups.ToDictionary(g => g.Id);

        string PathOf(DeviceGroup g)
        {
            var parts = new List<string>();
            DeviceGroup? current = g;
            while (current != null)
            {
                parts.Insert(0, current.Name);
                current = current.ParentGroupId == null ? null : byId[current.ParentGroupId.Value];
            }
            return string.Join(" / ", parts);
        }

        return groups.ToDictionary(g => g.Id, PathOf);
    }
}