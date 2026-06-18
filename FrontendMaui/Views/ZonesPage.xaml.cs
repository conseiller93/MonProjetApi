using FrontendMaui.ViewModels;

namespace FrontendMaui.Views;

public partial class ZonesPage : ContentPage
{
	public ZonesPage(ZonesViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as ZonesViewModel)?.GetZonesCommand.Execute(null);
    }
}
