using System.Windows.Input;
using FrontendMaui.DTOs;
using FrontendMaui.Models;
using FrontendMaui.Services;

namespace FrontendMaui.ViewModels;

public class RegisterViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    private string _identifiant = string.Empty;
    private string _motDePasse = string.Empty;
    private string _motDePasseConfirm = string.Empty;
    private RoleUtilisateur _roleSelectionne = RoleUtilisateur.Chauffeur;
    private string _errorMessage = string.Empty;
    private bool _hasError;

    public string Identifiant
    {
        get => _identifiant;
        set { _identifiant = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanRegister)); }
    }

    public string MotDePasse
    {
        get => _motDePasse;
        set { _motDePasse = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanRegister)); }
    }

    public string MotDePasseConfirm
    {
        get => _motDePasseConfirm;
        set { _motDePasseConfirm = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanRegister)); }
    }

    public RoleUtilisateur RoleSelectionne
    {
        get => _roleSelectionne;
        set { _roleSelectionne = value; OnPropertyChanged(); }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    public bool HasError
    {
        get => _hasError;
        set { _hasError = value; OnPropertyChanged(); }
    }

    public bool CanRegister =>
        !string.IsNullOrWhiteSpace(Identifiant) &&
        !string.IsNullOrWhiteSpace(MotDePasse) &&
        !string.IsNullOrWhiteSpace(MotDePasseConfirm);

    public List<RoleUtilisateur> Roles { get; } = Enum.GetValues<RoleUtilisateur>().ToList();

    public ICommand RegisterCommand { get; }
    public ICommand GoToLoginCommand { get; }

    public RegisterViewModel(ApiService apiService)
    {
        _apiService = apiService;
        RegisterCommand = new Command(async () => await RegisterAsync(), () => CanRegister && IsNotBusy);
        GoToLoginCommand = new Command(async () => await Shell.Current.GoToAsync("///LoginPage"));
    }

    private async Task RegisterAsync()
    {
        HasError = false;

        if (MotDePasse != MotDePasseConfirm)
        {
            ErrorMessage = "Les mots de passe ne correspondent pas.";
            HasError = true;
            return;
        }

        if (MotDePasse.Length < 6)
        {
            ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères.";
            HasError = true;
            return;
        }

        IsBusy = true;

        try
        {
            var dto = new UtilisateurCreateDto
            {
                Identifiant = Identifiant.Trim(),
                MotDePasse = MotDePasse,
                Role = RoleSelectionne
            };

            var succes = await _apiService.RegisterAsync(dto);

            if (succes)
            {
                await Shell.Current.DisplayAlert("Succès", "Compte créé avec succès. Vous pouvez maintenant vous connecter.", "OK");
                await Shell.Current.GoToAsync("///LoginPage");
            }
            else
            {
                ErrorMessage = "Échec de l'inscription. Identifiant peut-être déjà utilisé.";
                HasError = true;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur réseau : {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
