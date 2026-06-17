using System;
using FrontendMaui.ViewModels;
using Microsoft.Maui.Controls;

namespace FrontendMaui.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnRetourTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
