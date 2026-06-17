using System.Net.Http.Headers;
using System.Net.Http.Json;
using FrontendMaui.DTOs;
using Microsoft.Maui.Devices;

namespace FrontendMaui.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private const string TokenKey = "jwt_token";
    private const string RoleKey = "user_role";
    private const string UserIdKey = "user_id";

    public ApiService()
    {
        // Utiliser l'IP 10.0.2.2 si on est sur l'émulateur Android, sinon localhost
        string baseAddress = DeviceInfo.Platform == DevicePlatform.Android 
            ? "http://10.0.2.2:8080/api/" 
            : "http://localhost:8080/api/";

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseAddress)
        };
    }

    public async Task<(bool Success, string? ErrorMessage, SessionResponseDto? Session)> LoginAsync(string identifiant, string motDePasse)
    {
        try
        {
            var dto = new ConnexionDto { Identifiant = identifiant, MotDePasse = motDePasse };
            var response = await _httpClient.PostAsJsonAsync("Utilisateurs/login", dto);

            if (response.IsSuccessStatusCode)
            {
                var session = await response.Content.ReadFromJsonAsync<SessionResponseDto>();
                if (session != null && !string.IsNullOrEmpty(session.Token))
                {
                    await SecureStorage.Default.SetAsync(TokenKey, session.Token);
                    await SecureStorage.Default.SetAsync(RoleKey, session.Utilisateur.Role.ToString());
                    await SecureStorage.Default.SetAsync(UserIdKey, session.Utilisateur.Id.ToString());
                    
                    // Configurer le header Bearer par défaut pour les requêtes suivantes
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
                    
                    return (true, null, session);
                }
                return (false, "Réponse du serveur invalide.", null);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (false, "Identifiant ou mot de passe incorrect.", null);

            return (false, $"Erreur serveur ({(int)response.StatusCode}).", null);
        }
        catch (HttpRequestException)
        {
            return (false, "Impossible de joindre l'API. Vérifiez que le conteneur Docker est lancé.", null);
        }
        catch (Exception ex)
        {
            return (false, $"Erreur inattendue : {ex.Message}", null);
        }
    }

    public async Task<bool> AjouterTokenAuthorizationAsync()
    {
        var token = await SecureStorage.Default.GetAsync(TokenKey);
        if (string.IsNullOrEmpty(token)) return false;

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return true;
    }

    public void Deconnexion()
    {
        SecureStorage.Default.Remove(TokenKey);
        SecureStorage.Default.Remove(RoleKey);
        SecureStorage.Default.Remove(UserIdKey);
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<bool> RegisterAsync(DTOs.UtilisateurCreateDto dto)
    {
        try
        {
            // L'API expose l'endpoint POST api/Utilisateurs/inscrire
            var response = await _httpClient.PostAsJsonAsync("Utilisateurs/inscrire", dto);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public HttpClient Client => _httpClient;
}
