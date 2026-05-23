namespace grades_mvc.ViewModels;

public class GradeViewModel
{
    public int GradeId { get; set; }
    public int StudentId { get; set; }
    public string StudentFullName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public DateTime GradeDate { get; set; }
    public string? Notes { get; set; }
}
