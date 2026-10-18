using AquaAlerte.Models;

namespace AquaAlerte.Views;

public partial class ObservationRecuePage : ContentPage
{
    public ObservationRecuePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var s = DemoData.Selected;
        if (s == null) return;

        TitleLabel.Text = s.Titre;
        ConfLabel.Text = $"{s.Confirmations} confirmations, dont la vôtre";
        MyObsLabel.Text = $"Votre observation : {s.MaConfirmation}";
        StartLabel.Text = s.DebutConnu
            ? $"Début à {s.Debut} · {s.DureeCourte} observées"
            : "Début inconnu";
    }

    private async void OnBackToReportClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//AccueilPage/" + nameof(DetailCoupurePage));
    }

    private async void OnBackToQuartierClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//AccueilPage");
    }

    private async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}