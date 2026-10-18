using AquaAlerte.Helpers;
using AquaAlerte.Models;

namespace AquaAlerte.Views;

public partial class SignalerPage : ContentPage
{
    private string _quartier;
    private string _ville;
    private bool _knowsStart = true;
    private Signalement? _existing;

    public SignalerPage()
    {
        InitializeComponent();

        // Quartier choisi à l'étape précédente
        _quartier = Preferences.Default.Get("quartier", "Akwa");
        _ville = Preferences.Default.Get("ville", "Douala");

        // Valeurs par défaut : maintenant
        DatePick.MaximumDate = DateTime.Today;
        DatePick.Date = DateTime.Today;
        TimePick.Time = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);

        ApplyQuartier();
        SetKnow(true);
    }

    // ----- Quartier et coupure existante -----

    private void ApplyQuartier()
    {
        QuartierLabel.Text = _quartier;
        VilleLabel.Text = $"{_ville} · Cameroun";

        _existing = DemoData.Signalements.FirstOrDefault(s => s.EnCours && s.Quartier == _quartier);
        ExistingSection.IsVisible = _existing != null;
        if (_existing != null)
        {
            ExistingTitle.Text = $"Une coupure est déjà signalée à {_quartier}";
            ExistingText.Text = $"{_existing.Rue} : si c’est la même coupure, confirmez-la plutôt que de créer un doublon.";
        }
    }

    private async void OnChangeQuartierTapped(object? sender, TappedEventArgs e)
    {
        string[] names = DemoData.Villes.TryGetValue(_ville, out var list) ? list.ToArray() : Array.Empty<string>();
        string? choice = await DisplayActionSheetAsync("Quartier concerné", "Annuler", null, names);

        if (choice != null && names.Contains(choice))
        {
            _quartier = choice;
            ApplyQuartier();
        }
    }

    private async void OnSeeExistingClicked(object? sender, EventArgs e)
    {
        if (_existing == null) return;
        DemoData.Selected = _existing;
        await Shell.Current.GoToAsync(nameof(DetailCoupurePage));
    }

    // ----- « Savez-vous quand ça a commencé ? » -----

    private void OnKnowTapped(object? sender, TappedEventArgs e)
    {
        SetKnow(e.Parameter as string == "yes");
    }

    private void SetKnow(bool know)
    {
        _knowsStart = know;
        StyleChip(ChipKnow, ChipKnowLabel, know);
        StyleChip(ChipUnknown, ChipUnknownLabel, !know);
        DateTimeGrid.Opacity = know ? 1 : 0.4;
        DateTimeGrid.IsEnabled = know;
    }

    private static void StyleChip(Border chip, Label label, bool active)
    {
        chip.BackgroundColor = active ? FieldHelper.GetColor("AquaPrimary") : Colors.White;
        chip.Stroke = active ? FieldHelper.GetColor("AquaPrimary") : FieldHelper.GetColor("AquaBorder");
        label.TextColor = active ? Colors.White : FieldHelper.GetColor("AquaMuted");
    }

    // ----- Cadre bleu quand on tape dans un champ -----

    private void OnFieldFocused(object? sender, FocusEventArgs e) => SetFocusBorder(sender, true);
    private void OnFieldUnfocused(object? sender, FocusEventArgs e) => SetFocusBorder(sender, false);

    private static void SetFocusBorder(object? sender, bool on)
    {
        Element? el = sender as Element;
        while (el != null && el is not Border) el = el.Parent;   // remonte jusqu'au cadre
        if (el is Border box)
        {
            box.Stroke = FieldHelper.GetColor(on ? "AquaPrimary" : "AquaBorder");
            box.StrokeThickness = on ? 1.5 : 1;
        }
    }

    // ----- Boutons -----

    private async void OnUsePositionTapped(object? sender, TappedEventArgs e)
    {
        await DisplayAlertAsync("Position", "Cette fonction sera ajoutée plus tard.", "OK");
    }

    private async void OnContinueClicked(object? sender, EventArgs e)
{
    // Date et heure de début (si l'utilisateur les connaît)
    DateTime? debut = null;
    if (_knowsStart)
    {
        DateTime? date = DatePick.Date;
        TimeSpan? time = TimePick.Time;
        if (date.HasValue && time.HasValue)
            debut = date.Value.Date + time.Value;

        if (debut > DateTime.Now)
        {
            await DisplayAlertAsync("Heure invalide", "Le début ne peut pas être dans le futur.", "OK");
            return;
        }
    }

    // « Rue Joss, près de la pharmacie » : avant la virgule = rue, après = repère
    string raw = (StreetEntry.Text ?? "").Trim();
    string rue = raw;
    string repere = "";
    int i = raw.IndexOf(',');
    if (i >= 0)
    {
        rue = raw[..i].Trim();
        repere = raw[(i + 1)..].Trim();
    }

    DemoData.Brouillon = new BrouillonSignalement
    {
        Quartier = _quartier,
        Ville = _ville,
        Rue = rue,
        Repere = repere,
        Observation = (ObsEditor.Text ?? "").Trim(),
        Debut = debut
    };

    await Shell.Current.GoToAsync(nameof(RecapSignalementPage));
}

    private async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}