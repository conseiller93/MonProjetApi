using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using FrontendMaui.DTOs;
using FrontendMaui.Services;

namespace FrontendMaui.ViewModels;

public partial class ZonesViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<ZoneDto> Zones { get; } = new();

    public ZonesViewModel(ApiService apiService)
    {
        Title = "Gestion des Zones";
        _apiService = apiService;
    }

    [RelayCommand]
    async Task GetZonesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var zones = await _apiService.GetZonesAsync();

            if (Zones.Count != 0)
                Zones.Clear();

            foreach (var zone in zones)
                Zones.Add(zone);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erreur", $"Impossible de récupérer les zones: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
