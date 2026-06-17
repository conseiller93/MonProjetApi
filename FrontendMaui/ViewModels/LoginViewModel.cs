using System.Windows.Input;
using FrontendMaui.Services;

namespace FrontendMaui.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    private string _identifiant = string.Empty;
    public string Identifiant
    {
        get => _identifiant;
        set => SetProperty(ref _identifiant, value);
    }

    private string _motDePasse = string.Empty;
    public string MotDePasse
    {
        get => _motDePasse;
        set => SetProperty(ref _motDePasse, value);
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(IsNotBusy));
                ((Command)LoginCommand).ChangeCanExecute();
            }
        }
    }

    public bool IsNotBusy => !IsBusy;

    public ICommand LoginCommand { get; }

    public LoginViewModel(ApiService apiService)
    {
        _apiService = apiService;
        LoginCommand = new Command(async () => await LoginAsync(), () => !IsBusy);
    }

    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Identifiant) || string.IsNullOrWhiteSpace(MotDePasse))
        {
            ErrorMessage = "Veuillez remplir tous les champs.";
            return;
        }

        IsBusy = true;

        var (success, error, session) = await _apiService.LoginAsync(Identifiant, MotDePasse);

        IsBusy = false;

        if (!success)
        {
            ErrorMessage = error ?? "Erreur de connexion.";
            return;
        }

        // Navigation vers le Dashboard (sera défini dans AppShell)
        await Shell.Current.GoToAsync("//DashboardPage");
    }
}
