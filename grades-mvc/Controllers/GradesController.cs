using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using grades_mvc.Data;
using grades_mvc.Models;
using grades_mvc.Services;
using grades_mvc.ViewModels;

namespace grades_mvc.Controllers;

public class GradesController : Controller
{
    private readonly AppDbContext _db;
    private readonly StudentsApiClient _studentsApi;

    public GradesController(AppDbContext db, StudentsApiClient studentsApi)
    {
        _db = db;
        _studentsApi = studentsApi;
    }

    public async Task<IActionResult> Index()
    {
        var grades = await _db.Grades.ToListAsync();
        var students = await _studentsApi.GetAllStudentsAsync();

        bool studentsUnavailable = students.Count == 0;
        if (studentsUnavailable)
            ViewBag.Warning = "⚠️ Students service is unavailable. Showing Student IDs instead of names.";

        var viewModels = grades.Select(g => new GradeViewModel
        {
            GradeId = g.Id,
            StudentId = g.StudentId,
            StudentFullName = students.FirstOrDefault(s => s.Id == g.StudentId)?.FullName
                             ?? $"Student #{g.StudentId}",
            CourseName = g.CourseName,
            Score = g.Score,
            GradeDate = g.GradeDate,
            Notes = g.Notes
        }).ToList();

        return View(viewModels);
    }

    public async Task<IActionResult> Details(int id)
    {
        var grade = await _db.Grades.FindAsync(id);
        if (grade == null) return NotFound();

        var student = await _studentsApi.GetStudentByIdAsync(grade.StudentId);

        var vm = new GradeViewModel
        {
            GradeId = grade.Id,
            StudentId = grade.StudentId,
            StudentFullName = student?.FullName ?? $"Student #{grade.StudentId}",
            CourseName = grade.CourseName,
            Score = grade.Score,
            GradeDate = grade.GradeDate,
            Notes = grade.Notes
        };

        return View(vm);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateStudentsDropdownAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Grade grade)
    {
        if (!ModelState.IsValid)
        {
            await PopulateStudentsDropdownAsync();
            return View(grade);
        }

        _db.Grades.Add(grade);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var grade = await _db.Grades.FindAsync(id);
        if (grade == null) return NotFound();

        await PopulateStudentsDropdownAsync(selectedStudentId: grade.StudentId);
        return View(grade);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Grade grade)
    {
        if (id != grade.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await PopulateStudentsDropdownAsync(selectedStudentId: grade.StudentId);
            return View(grade);
        }

        _db.Update(grade);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var grade = await _db.Grades.FindAsync(id);
        if (grade == null) return NotFound();

        var student = await _studentsApi.GetStudentByIdAsync(grade.StudentId);

        var vm = new GradeViewModel
        {
            GradeId = grade.Id,
            StudentId = grade.StudentId,
            StudentFullName = student?.FullName ?? $"Student #{grade.StudentId}",
            CourseName = grade.CourseName,
            Score = grade.Score,
            GradeDate = grade.GradeDate,
            Notes = grade.Notes
        };

        return View(vm);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var grade = await _db.Grades.FindAsync(id);
        if (grade != null)
        {
            _db.Grades.Remove(grade);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateStudentsDropdownAsync(int? selectedStudentId = null)
    {
        var students = await _studentsApi.GetAllStudentsAsync();
        ViewBag.Students = new SelectList(students, "Id", "FullName", selectedStudentId);
    }
}
