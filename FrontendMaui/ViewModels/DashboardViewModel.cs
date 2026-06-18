using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using FrontendMaui.Services;
using System.Net.Http.Json;

namespace FrontendMaui.ViewModels;

public class DashboardViewModel : BaseViewModel
{

    private bool _isAdmin;
    public bool IsAdmin
    {
        get => _isAdmin;
        set => SetProperty(ref _isAdmin, value);
    }

    // Données affichées
    private int _nombreChargements;
    public int NombreChargements { get => _nombreChargements; set => SetProperty(ref _nombreChargements, value); }

    private decimal _carburantTotal;
    public decimal CarburantTotal { get => _carburantTotal; set => SetProperty(ref _carburantTotal, value); }

    private decimal _revenuTotal;
    public decimal RevenuTotal { get => _revenuTotal; set => SetProperty(ref _revenuTotal, value); }

    // Commandes
    public ICommand RefreshCommand { get; }

    private readonly ApiService _apiService;

    public DashboardViewModel(ApiService apiService)
    {
        _apiService = apiService;
        RefreshCommand = new Command(async () => await LoadStatsAsync());
    }

    public async Task LoadStatsAsync()
    {
        try
        {
            // call API statistics endpoint
            var client = _apiService.Client;
            var stats = await client.GetFromJsonAsync<FrontendMaui.DTOs.StatistiqueJournaliereDto>("Statistiques/journalieres");
            if (stats != null)
            {
                NombreChargements = stats.Global.NombreChargements;
                CarburantTotal = stats.Global.CarburantTotalLitres;
                RevenuTotal = stats.Global.RevenuTotalGNF;
            }
        }
        catch
        {
            // ignore pour l'instant
        }
    }
}
