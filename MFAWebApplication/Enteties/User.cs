namespace AuthenticationWebApplication.Enteties
{
    public class User
    {
        // Properties 
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone {  get; set; } = string.Empty;
        public UserGenderEnum Gender { get; set;  }

        public string? MfaSecretKey { get; set; }
        public bool IsMfaEnabled { get; set; } = false;
    }
}
