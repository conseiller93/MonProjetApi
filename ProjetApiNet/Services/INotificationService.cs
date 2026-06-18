using System.Threading.Tasks;
using ProjetApiNet.Models;

namespace ProjetApiNet.Services;

public interface INotificationService
{
    Task SendConfirmationAsync(Utilisateur utilisateur);
}
