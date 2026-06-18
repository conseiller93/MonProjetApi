using FrontendMaui.ViewModels;

namespace FrontendMaui.Views;

public partial class ChauffeursPage : ContentPage
{
	public ChauffeursPage(ChauffeursViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as ChauffeursViewModel)?.GetChauffeursCommand.Execute(null);
    }
}
