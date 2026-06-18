using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FrontendMaui.Models;
using FrontendMaui.Services;

namespace FrontendMaui.ViewModels;

public partial class StatisticsViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    int totalChargements;

    [ObservableProperty]
    decimal totalCarburant;

    [ObservableProperty]
    decimal totalPrimes;

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

            TotalChargements = chargements.Count;
            TotalCarburant = chargements.Sum(c => c.CarburantTotalLitres);
            TotalPrimes = chargements.Sum(c => c.PrimeChauffeurGNF + c.PrimeSuperviseurGroupeGNF + c.PrimeSuperviseurZoneGNF + c.PrimeSupervGenGNF);
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
