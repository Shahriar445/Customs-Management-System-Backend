namespace Customs_Management_System.DTOs
{
    public class LoginRequestDto
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // Add this line
    }
}
