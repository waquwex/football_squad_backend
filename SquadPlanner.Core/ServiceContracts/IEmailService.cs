namespace FootballSquad.Core.ServiceContracts
{
    public interface IEmailService
    {
        void SendForgotPasswordEmail(string email, string token);
    }
}
