using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProjetApiNet.Models;

namespace ProjetApiNet.Services;

public class DebugNotificationService : INotificationService
{
    private readonly ILogger<DebugNotificationService> _logger;

    public DebugNotificationService(ILogger<DebugNotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendConfirmationAsync(Utilisateur utilisateur)
    {
        // Pour l'instant on se contente de logguer l'URL de confirmation
        var url = $"/api/confirm/confirm/{utilisateur.ConfirmationToken}";
        _logger.LogInformation("[DEBUG NOTIF] Confirmation for {Identifiant}: {Url}", utilisateur.Identifiant, url);
        return Task.CompletedTask;
    }
}
