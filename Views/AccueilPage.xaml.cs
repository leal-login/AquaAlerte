using AquaAlerte.Models;

namespace AquaAlerte.Views;

public partial class AccueilPage : ContentPage
{
    public AccueilPage()
    {
        InitializeComponent();
    }

    // Appelé à chaque fois que la page s'affiche : le quartier a pu changer
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Refresh();
    }

    private void Refresh()
    {
        string quartier = Preferences.Default.Get("quartier", "Akwa");
        string ville = Preferences.Default.Get("ville", "Douala");

        QuartierLabel.Text = quartier;
        VilleLabel.Text = $"{ville} · Cameroun";
        SectionCurrentLabel.Text = $"En cours à {quartier}";

        var current = DemoData.Signalements.Where(s => s.EnCours && s.Quartier == quartier).ToList();
        var nearby = DemoData.Signalements.Where(s => s.EnCours && s.Quartier != quartier).ToList();

        OutageCountLabel.Text = current.Count.ToString();
        OutageTextLabel.Text = current.Count > 1 ? "coupures en cours" : "coupure en cours";
        ConfCountLabel.Text = current.Sum(s => s.Confirmations).ToString();

        BindableLayout.SetItemsSource(CurrentLayout, current);
        BindableLayout.SetItemsSource(NearbyLayout, nearby);

        NoCurrentLabel.Text = $"Aucune coupure en cours signalée à {quartier}.";
        NoCurrentLabel.IsVisible = current.Count == 0;
        NearbySection.IsVisible = nearby.Count > 0;
    }

    private async void OnChangeQuartierTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(QuartierPage));
    }

    private async void OnReportClicked(object? sender, EventArgs e)
    {
    await Shell.Current.GoToAsync(nameof(SignalerPage));
    }

    private async void OnSeeMapTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//CartePage");
    }

   // Toucher une carte ouvre le détail de la coupure
private async void OnCardTapped(object? sender, TappedEventArgs e)
{
    if (sender is Border { BindingContext: Signalement s })
    {
        DemoData.Selected = s;
        await Shell.Current.GoToAsync(nameof(DetailCoupurePage));
    }
}
}