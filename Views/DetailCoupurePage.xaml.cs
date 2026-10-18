using AquaAlerte.Models;

namespace AquaAlerte.Views;

public partial class DetailCoupurePage : ContentPage
{
    public DetailCoupurePage()
    {
        InitializeComponent();
    }

    // À chaque affichage, on recharge la coupure (elle a pu changer)
    protected override void OnAppearing()
    {
        base.OnAppearing();

        BindingContext = null;   // force la mise à jour des textes
        BindingContext = DemoData.Selected ?? DemoData.Signalements.First();
    }

    private async void OnStillCutClicked(object? sender, EventArgs e)
    {
        if (DemoData.Selected is not Signalement s) return;

        if (!DemoData.Confirmer(s))
        {
            await DisplayAlertAsync("Déjà confirmée", "Vous avez déjà confirmé cette coupure.", "OK");
            return;
        }

        await Shell.Current.GoToAsync(nameof(ObservationRecuePage));
    }

    private async void OnWaterBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RetourEauPage));
    }

    private async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}