using IOT_Devices.Data;
using IOT_Devices.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IOT_Devices.Controllers;

public class FirmwareController : Controller
{
    private readonly AppDbContext _db;

    public FirmwareController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /Firmware
    public async Task<IActionResult> Index()
    {
        var items = await _db.Firmware
            .Include(f => f.DeviceType)
            .Include(f => f.Devices)
            .OrderBy(f => f.DeviceType!.Name)
            .ThenByDescending(f => f.ReleaseDate)
            .ToListAsync();
        return View(items);
    }

    // GET: /Firmware/Create
    public async Task<IActionResult> Create()
    {
        await LoadTypeOptions();
        return View(new Firmware { ReleaseDate = DateOnly.FromDateTime(DateTime.Today) });
    }

    // POST: /Firmware/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("DeviceTypeId,Version,ReleaseDate,Notes")] Firmware firmware)
    {
        firmware.Version = (firmware.Version ?? "").Trim();

        if (!await _db.DeviceTypes.AnyAsync(t => t.Id == firmware.DeviceTypeId))
        {
            ModelState.AddModelError("DeviceTypeId", "Choose a device type. ");
        }

        bool duplicate = await _db.Firmware.AnyAsync(f =>
            f.DeviceTypeId == firmware.DeviceTypeId && f.Version == firmware.Version);
        if (duplicate)
        {
            ModelState.AddModelError("Version", "Device already has firmware version. ");
        }

        if (!ModelState.IsValid)
        {
            await LoadTypeOptions(firmware.DeviceTypeId);
            return View(firmware);
        }

        _db.Firmware.Add(firmware);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Firmware/Edit/ID
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var firmware = await _db.Firmware.Include(f => f.DeviceType)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (firmware == null) return NotFound();

        ViewBag.TypeName = firmware.DeviceType!.Name;
        return View(firmware);
    }

    // POST: /Firmware/Edit/ID
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id, [Bind("Id,Version,ReleaseDate,Notes")] Firmware input)
    {
        if (id != input.Id) return NotFound();
        var firmware = await _db.Firmware.Include(f => f.DeviceType)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (firmware == null) return NotFound();

        input.Version = (input.Version ?? "").Trim();

        bool duplicate = await _db.Firmware.AnyAsync(f =>
            f.Id != id && f.DeviceTypeId == firmware.DeviceTypeId && f.Version == input.Version);
        if (duplicate)
        {
            ModelState.AddModelError("Version", "Device already has firmware version.");
        }

        if (!ModelState.IsValid)
        {
            input.DeviceTypeId = firmware.DeviceTypeId;
            ViewBag.TypeName = firmware.DeviceType!.Name;
            return View(input);
        }

        firmware.Version = input.Version;
        firmware.ReleaseDate = input.ReleaseDate;
        firmware.Notes = input.Notes;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Firmware/Delete/ID
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var firmware = await _db.Firmware
            .Include(f => f.DeviceType)
            .Include(f => f.Devices)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (firmware == null) return NotFound();
        return View(firmware);
    }

    // POST: /Firmware/Delete/ID
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var firmware = await _db.Firmware.Include(f => f.Devices)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (firmware == null) return RedirectToAction(nameof(Index));

        if (firmware.Devices.Any())
        {
            return RedirectToAction(nameof(Delete), new { id });
        }

        _db.Firmware.Remove(firmware);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadTypeOptions(int? selectedId = null)
    {
        var types = await _db.DeviceTypes.OrderBy(t => t.Name).ToListAsync();
        ViewBag.TypeOptions = new SelectList(types, "Id", "Name", selectedId);
    }
}