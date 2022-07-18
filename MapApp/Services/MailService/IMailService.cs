using MapApp.Models.QueryModels;
using System.Threading.Tasks;

namespace MapApp.Services.MailService
{
    public interface IMailService
    {
        Task SendEmailAsync(MailRequest mailRequest);
    }
}
