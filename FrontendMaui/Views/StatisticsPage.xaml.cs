using FrontendMaui.ViewModels;

namespace FrontendMaui.Views;

public partial class StatisticsPage : ContentPage
{
	public StatisticsPage(StatisticsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as StatisticsViewModel)?.GetStatisticsCommand.Execute(null);
    }
}
