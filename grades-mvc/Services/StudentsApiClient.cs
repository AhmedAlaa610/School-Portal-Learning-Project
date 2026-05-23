using System.Net.Http.Json;

namespace grades_mvc.Services;

public class StudentsApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<StudentsApiClient> _logger;

    public StudentsApiClient(HttpClient http, ILogger<StudentsApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<StudentDto>> GetAllStudentsAsync()
    {
        try
        {
            var students = await _http.GetFromJsonAsync<List<StudentDto>>("/api/students");
            return students ?? new List<StudentDto>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Students service unavailable: {Message}", ex.Message);
            return new List<StudentDto>();
        }
    }

    public async Task<StudentDto?> GetStudentByIdAsync(int id)
    {
        var all = await GetAllStudentsAsync();
        return all.FirstOrDefault(s => s.Id == id);
    }
}
