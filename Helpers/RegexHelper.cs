namespace backend.Helpers
{
    public static class RegexHelper
    {
        public const string Mobile = @"^[0-9]{10}$";
        public const string Password = @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{6,}$";
        public const string Name = @"^[a-zA-Z]+$";
    }   
}