namespace Blanquita.Application.DTOs;

public class UpdateUserDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
    public string Role { get; set; } = string.Empty;
    public int? BranchId { get; set; }
}
