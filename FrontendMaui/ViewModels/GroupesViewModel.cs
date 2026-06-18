using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using FrontendMaui.DTOs;
using FrontendMaui.Services;

namespace FrontendMaui.ViewModels;

public partial class GroupesViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<GroupeDto> Groupes { get; } = new();

    public GroupesViewModel(ApiService apiService)
    {
        Title = "Gestion des Groupes";
        _apiService = apiService;
    }

    [RelayCommand]
    async Task GetGroupesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var groupes = await _apiService.GetGroupesAsync();

            if (Groupes.Count != 0)
                Groupes.Clear();

            foreach (var groupe in groupes)
                Groupes.Add(groupe);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erreur", $"Impossible de récupérer les groupes: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
