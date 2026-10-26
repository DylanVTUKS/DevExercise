using IOT_Devices.Data;
using IOT_Devices.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IOT_Devices.Controllers;

public class GroupsController : Controller
{
    private readonly AppDbContext _db;

    public GroupsController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        //Load data
        var groups = await _db.DeviceGroups
            .Include(g => g.Devices)
            .ThenInclude(d => d.Firmware)
            .ToListAsync();
        
        var roots = groups
            .Where(g => g.ParentGroupId == null)
            .OrderBy(g => g.Name)
            .ToList();

        return View(roots);
    }

    // GET: /Groups/Create  
    public async Task<IActionResult> Create()
    {
        await LoadParentOptions();
        return View(new DeviceGroup());
    }

    // POST: /Groups/Create 
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,ParentGroupId")] DeviceGroup group)
    {
        bool duplicate = await _db.DeviceGroups
            .AnyAsync(g => g.ParentGroupId == group.ParentGroupId && g.Name == group.Name);
        if (duplicate)
        {   
            ModelState.AddModelError("Name", "Group with this name is already under the parent. ");
        }

        if (!ModelState.IsValid)
        {
            await LoadParentOptions(group.ParentGroupId);
            return View(group);
        }

        _db.DeviceGroups.Add(group);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
   
   // GET: /Groups/Edit/ID
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var group = await _db.DeviceGroups.FindAsync(id);
        if (group == null) return NotFound();

        await LoadParentOptions(group.ParentGroupId, excludeId: group.Id);
        return View(group);
    }

    // POST: /Groups/Edit/ID
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ParentGroupId")] DeviceGroup input)
    {
        if (id != input.Id) return NotFound();
        var group = await _db.DeviceGroups.FindAsync(id);
        if (group == null) return NotFound();

        // Cycle check
        if (input.ParentGroupId != null)
        {
            var all = await _db.DeviceGroups.AsNoTracking().ToListAsync();
            var blocked = GetDescendantIds(all, id);
            blocked.Add(id);
            if (blocked.Contains(input.ParentGroupId.Value))
            {
                ModelState.AddModelError("ParentGroupId",
                    "A group cannot be under itself. ");
            }
        }

        // Duplicate sibling name 
        bool duplicate = await _db.DeviceGroups.AnyAsync(g =>
            g.Id != id && g.ParentGroupId == input.ParentGroupId && g.Name == input.Name);
        if (duplicate)
        {
            ModelState.AddModelError("Name", "Group with this name exists under the tree group. ");
        }

        if (!ModelState.IsValid)
        {
            await LoadParentOptions(input.ParentGroupId, excludeId: id);
            return View(input);
        }

        group.Name = input.Name;
        group.ParentGroupId = input.ParentGroupId;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Groups/Delete/ID
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var group = await _db.DeviceGroups
            .Include(g => g.Children)
            .Include(g => g.Devices)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (group == null) return NotFound();
        return View(group);
    }

    // POST: /Groups/Delete/ID
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var group = await _db.DeviceGroups
            .Include(g => g.Children)
            .Include(g => g.Devices)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (group == null) return RedirectToAction(nameof(Index));

        if (group.Children.Any() || group.Devices.Any())
        {
            return RedirectToAction(nameof(Delete), new { id });
        }

        _db.DeviceGroups.Remove(group);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadParentOptions(int? selectedId = null, int? excludeId = null)
    {
        var groups = await _db.DeviceGroups.AsNoTracking().OrderBy(g => g.Name).ToListAsync();

        if (excludeId != null)
        {
            var blocked = GetDescendantIds(groups, excludeId.Value);
            blocked.Add(excludeId.Value);
            groups = groups.Where(g => !blocked.Contains(g.Id)).ToList();
        }

        ViewBag.ParentOptions = new SelectList(groups, "Id", "Name", selectedId);
    }
    private static HashSet<int> GetDescendantIds(List<DeviceGroup> all, int id)
    {
        var result = new HashSet<int>();
        var queue = new Queue<int>();
        queue.Enqueue(id);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var child in all.Where(g => g.ParentGroupId == current))
            {
                if (result.Add(child.Id)) queue.Enqueue(child.Id);
            }
        }
        return result;
    }
}