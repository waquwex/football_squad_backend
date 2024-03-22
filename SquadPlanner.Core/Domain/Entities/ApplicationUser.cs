namespace FootballSquad.Core.Domain.Entities
{
    public class ApplicationUser
    {
        public Guid? Id { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? HashedPassword { get; set; }
        public string? Salt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpirationDateTime { get; set; }
        public DateTime? ForgotPasswordRequestTime { get; set; }
        public DateTime? FailedLoginTime { get; set; }
        public int FailedLoginCount { get; set; }
        public int PassedSecondsFromLastFail { get; set; }
        public int PassedSecondsFromLastForgotPassword { get; set; }
        public string? ForgotPasswordToken { get; set; }
    }
}