namespace grades_mvc.Models;

public class Grade
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public decimal Score { get; set; }

    public DateTime GradeDate { get; set; }

    public string? Notes { get; set; }
}
