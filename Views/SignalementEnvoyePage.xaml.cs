using AquaAlerte.Models;

namespace AquaAlerte.Views;

public partial class SignalementEnvoyePage : ContentPage
{
    public SignalementEnvoyePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var s = DemoData.Selected;
        if (s == null) return;

        TitleLabel.Text = s.Titre;
        StartLabel.Text = s.DebutConnu
            ? $"Début observé à {s.Debut} · {s.DateLongue}"
            : "Début inconnu";
        PublishedLabel.Text = $"Publié à {s.MiseAJour} · votre observation";
        FollowSwitch.IsToggled = s.Suivie;
    }

    private void OnFollowToggled(object? sender, ToggledEventArgs e)
    {
        if (DemoData.Selected != null) DemoData.Selected.Suivie = e.Value;
    }

    // « Voir mon signalement » : le détail s'ouvre à la place de cet écran
    private async void OnSeeClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//AccueilPage/" + nameof(DetailCoupurePage));
    }

    private async void OnBackToQuartierClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//AccueilPage");
    }

    private async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//AccueilPage");
    }
}