using FrontendMaui.ViewModels;

namespace FrontendMaui.Views;

public partial class CamionsPage : ContentPage
{
    private readonly CamionsViewModel _vm;

    public CamionsPage(CamionsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _vm.LoadAsync();
    }
}
