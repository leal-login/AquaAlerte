using AquaAlerte.Models;

namespace AquaAlerte.Views;

public partial class DetailCoupurePage : ContentPage
{
    public DetailCoupurePage()
    {
        InitializeComponent();
    }

    // La page s'affiche avec la coupure choisie (BindingContext = la source des données)
    protected override void OnAppearing()
    {
        base.OnAppearing();
        BindingContext = DemoData.Selected ?? DemoData.Signalements.First();
    }

    private async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}