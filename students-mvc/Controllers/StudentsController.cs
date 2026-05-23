using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using students_mvc.Data;
using students_mvc.Models;

namespace students_mvc.Controllers;

public class StudentsController : Controller
{
    private readonly AppDbContext _db;

    public StudentsController(AppDbContext db)
    {
        _db = db;
    }
    public async Task<IActionResult> Index()
    {
        var students = await _db.Students.ToListAsync();
        return View(students);
    }

    public async Task<IActionResult> Details(int id)
    {
        var student = await _db.Students.FindAsync(id);
        if (student == null) return NotFound();
        return View(student);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Student student)
    {
        if (!ModelState.IsValid) return View(student);
        _db.Students.Add(student);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var student = await _db.Students.FindAsync(id);
        if (student == null) return NotFound();
        return View(student);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Student student)
    {
        if (id != student.Id) return BadRequest();
        if (!ModelState.IsValid) return View(student);

        _db.Update(student);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var student = await _db.Students.FindAsync(id);
        if (student == null) return NotFound();
        return View(student);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var student = await _db.Students.FindAsync(id);
        if (student != null)
        {
            _db.Students.Remove(student);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

  
    [HttpGet("/api/students")]
    public async Task<IActionResult> GetStudentsJson()
    {
        var students = await _db.Students
            .Select(s => new 
            { 
                s.Id,
                s.FirstName,
                s.LastName,
                FullName = s.FirstName + " " + s.LastName 
            })
            .ToListAsync();

        return Json(students);
    }
}
