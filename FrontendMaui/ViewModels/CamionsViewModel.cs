using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Windows.Input;
using FrontendMaui.DTOs;
using FrontendMaui.Services;

namespace FrontendMaui.ViewModels;

public class CamionsViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<CamionDto> Camions { get; } = new();

    private CamionDto? _selectedCamion;
    public CamionDto? SelectedCamion
    {
        get => _selectedCamion;
        set => SetProperty(ref _selectedCamion, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand LoadCommand { get; }

    public CamionsViewModel(ApiService apiService)
    {
        _apiService = apiService;
        RefreshCommand = new Command(async () => await LoadAsync());
        LoadCommand = new Command(async () => await LoadAsync());
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var client = _apiService.Client;
            var list = await client.GetFromJsonAsync<List<CamionDto>>("Camions");
            Camions.Clear();
            if (list != null)
            {
                foreach (var c in list)
                    Camions.Add(c);
            }
        }
        catch
        {
            // ignore for now
        }
        finally
        {
            IsBusy = false;
        }
    }
}
