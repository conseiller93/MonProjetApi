using FrontendMaui.ViewModels;

namespace FrontendMaui.Views;

public partial class GroupesPage : ContentPage
{
	public GroupesPage(GroupesViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as GroupesViewModel)?.GetGroupesCommand.Execute(null);
    }
}
