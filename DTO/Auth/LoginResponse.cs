namespace backend.DTOs.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public UserResponse User { get; set; } = null!;
        public string? Role { get; set; }= null!;
        public bool IsActive { get; set; }
    }
}