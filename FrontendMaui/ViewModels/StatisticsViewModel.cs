using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FrontendMaui.Models;
using System.Linq;
using FrontendMaui.Services;

namespace FrontendMaui.ViewModels;

public partial class StatisticsViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private int _totalChargements;

    [ObservableProperty]
    private decimal _totalCarburant;

    [ObservableProperty]
    private decimal _totalPrimes;

    public StatisticsViewModel(ApiService apiService)
    {
        Title = "Statistiques Globales";
        _apiService = apiService;
    }

    [RelayCommand]
    async Task GetStatisticsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var chargements = await _apiService.GetChargementsAsync();

            _totalChargements = chargements.Count;
            _totalCarburant = chargements.Sum(c => c.CarburantTotalLitres);
            _totalPrimes = chargements.Sum(c => c.PrimeChauffeurGNF + c.PrimeSuperviseurGroupeGNF + c.PrimeSuperviseurZoneGNF + c.PrimeSupervGenGNF);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erreur", $"Impossible de récupérer les statistiques: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
