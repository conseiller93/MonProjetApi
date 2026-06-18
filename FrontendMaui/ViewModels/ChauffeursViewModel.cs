using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using FrontendMaui.Models;
using FrontendMaui.Services;

namespace FrontendMaui.ViewModels;

public partial class ChauffeursViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<Utilisateur> Chauffeurs { get; } = new();

    public ChauffeursViewModel(ApiService apiService)
    {
        Title = "Gestion des Chauffeurs";
        _apiService = apiService;
    }

    [RelayCommand]
    async Task GetChauffeursAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var chauffeurs = await _apiService.GetChauffeursAsync();

            if (Chauffeurs.Count != 0)
                Chauffeurs.Clear();

            foreach (var chauffeur in chauffeurs)
                Chauffeurs.Add(chauffeur);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erreur", $"Impossible de récupérer les chauffeurs: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
