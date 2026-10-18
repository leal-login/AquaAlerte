using AquaAlerte.Helpers;
using AquaAlerte.Models;

namespace AquaAlerte.Views;

public partial class RetourEauPage : ContentPage
{
    private bool _now = true;   // true = « À l'instant », false = « Une autre heure »

    public RetourEauPage()
    {
        InitializeComponent();

        // Met à jour la phrase « Aujourd'hui, ... » quand l'heure change
        TimePick.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TimePicker.Time)) UpdateWhenLabel();
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var s = DemoData.Selected;
        if (s == null) return;

        SubLabel.Text = s.Titre;
        StartTitleLabel.Text = s.DebutConnu ? $"Coupure débutée à {s.Debut}" : "Début inconnu";
        StartSubLabel.Text = s.DebutConnu
            ? $"{s.DateComplete} · {s.DureeCourte} observées à {s.HeureRef}"
            : "La durée sera indiquée comme inconnue.";

        NoteEditor.Text = "";
        TimePick.Time = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
        SetWhen(true);
    }

    // ----- « À l'instant » ou « Une autre heure » -----

    private void OnWhenTapped(object? sender, TappedEventArgs e)
    {
        SetWhen(e.Parameter as string == "now");
    }

    private void SetWhen(bool now)
    {
        _now = now;
        StyleChip(ChipNow, ChipNowLabel, now);
        StyleChip(ChipOther, ChipOtherLabel, !now);
        TimeBox.IsVisible = !now;
        UpdateWhenLabel();
    }

    private static void StyleChip(Border chip, Label label, bool active)
    {
        chip.BackgroundColor = active ? FieldHelper.GetColor("AquaPrimary") : Colors.White;
        chip.Stroke = active ? FieldHelper.GetColor("AquaPrimary") : FieldHelper.GetColor("AquaBorder");
        label.TextColor = active ? Colors.White : FieldHelper.GetColor("AquaMuted");
    }

    private DateTime ChosenTime()
    {
        if (_now) return DateTime.Now;

        TimeSpan? t = TimePick.Time;
        return t.HasValue ? DateTime.Today + t.Value : DateTime.Now;
    }

    private void UpdateWhenLabel()
    {
        DateTime d = ChosenTime();
        WhenLabel.Text = $"Aujourd’hui, {d.ToString("d MMMM", TimeHelper.Fr)} · {TimeHelper.Heure(d)}";
    }

    // ----- Publier -----

    private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        var s = DemoData.Selected;
        if (s == null) return;

        DateTime now = DateTime.Now;
        DateTime fin = ChosenTime();

        if (fin > now.AddMinutes(1))
        {
            await DisplayAlertAsync("Heure invalide", "Le retour de l’eau ne peut pas être dans le futur.", "OK");
            return;
        }

        TimeSpan? duree = s.DureeJusqua(fin);
        if (!_now && s.DebutConnu && duree == null)
        {
            await DisplayAlertAsync("Heure invalide", "Le retour ne peut pas être avant le début de la coupure.", "OK");
            return;
        }

        string note = (NoteEditor.Text ?? "").Trim();
        string heureFin = TimeHelper.Heure(fin);

        s.EnCours = false;
        s.Fin = heureFin;
        s.MiseAJour = TimeHelper.Heure(now);
        s.DerniereObs = $"eau revenue à {heureFin}";
        if (duree.HasValue) s.DureeCourte = TimeHelper.FormatDuree(duree.Value);
        s.Activites.Insert(0, new Activite
        {
            Titre = "Eau revenue",
            Detail = note.Length > 0 ? note : "Vous observez de l’eau au robinet.",
            Heure = heureFin
        });

        await DisplayAlertAsync("Merci", "Votre observation est publiée. Le quartier est informé du retour de l’eau.", "OK");
        await Shell.Current.GoToAsync("//AccueilPage/" + nameof(DetailCoupurePage));
    }

    private async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}