using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }
}
