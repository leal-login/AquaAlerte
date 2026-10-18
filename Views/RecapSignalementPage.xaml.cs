using AquaAlerte.Helpers;
using AquaAlerte.Models;

namespace AquaAlerte.Views;

public partial class RecapSignalementPage : ContentPage
{
    private BrouillonSignalement? _draft;
    private bool _checked = true;

    public RecapSignalementPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _draft = DemoData.Brouillon;
        if (_draft == null) return;

        DateTime now = DateTime.Now;

        TitleLabel.Text = _draft.Titre;
        SubLabel.Text = string.IsNullOrEmpty(_draft.Repere)
            ? _draft.Ville
            : $"{_draft.Ville} · {_draft.Repere}";

        if (_draft.Debut is DateTime debut)
        {
            StartLabel.Text = $"Début observé à {TimeHelper.Heure(debut)}";
            DurationLabel.Text = $"{debut.ToString("d MMMM yyyy", TimeHelper.Fr)} · " +
                                 $"{TimeHelper.FormatDuree(now - debut)} observées à {TimeHelper.Heure(now)}";
        }
        else
        {
            StartLabel.Text = "Début inconnu";
            DurationLabel.Text = "La durée sera indiquée comme inconnue.";
        }

        ObsSection.IsVisible = _draft.Observation.Length > 0;
        ObsLabel.Text = $"« {_draft.Observation} »";

        SetChecked(true);
    }

    private void OnCheckTapped(object? sender, TappedEventArgs e) => SetChecked(!_checked);

    // Case cochée : bouton bleu. Décochée : bouton gris.
    private void SetChecked(bool value)
    {
        _checked = value;
        CheckIcon.IsVisible = value;
        CheckBox.BackgroundColor = value ? FieldHelper.GetColor("AquaInfoBg") : Colors.White;
        FieldHelper.SetButton(PublishButton, value);
    }

    private async void OnPublishClicked(object? sender, EventArgs e)
    {
        if (!_checked || _draft == null) return;

        DateTime now = DateTime.Now;
        DateTime? debut = _draft.Debut;
        int mine = DemoData.Signalements.Count(s => s.EstMien);

        var s = new Signalement
        {
            Quartier = _draft.Quartier,
            Ville = _draft.Ville,
            Rue = _draft.RueOuDefaut,
            EnCours = true,
            Confirmations = 1,
            DebutConnu = debut.HasValue,
            Debut = debut.HasValue ? TimeHelper.Heure(debut.Value) : "",
            DateTexte = debut.HasValue ? debut.Value.ToString("d MMM yyyy", TimeHelper.Fr) : "",
            DateLongue = debut.HasValue ? debut.Value.ToString("d MMMM", TimeHelper.Fr) : "",
            DureeCourte = debut.HasValue ? TimeHelper.FormatDuree(now - debut.Value) : "",
            MiseAJour = TimeHelper.Heure(now),
            HeureRef = TimeHelper.Heure(now),
            DerniereObs = $"toujours coupée à {TimeHelper.Heure(now)}",
            Note = _draft.Observation,
            Suivie = true,
            EstMien = true,
             DateComplete = debut.HasValue ? debut.Value.ToString("d MMMM yyyy", TimeHelper.Fr) : "",
            DebutReel = debut,
            ConfirmeParMoi = true,
            MaConfirmation = $"{now.ToString("d MMM", TimeHelper.Fr)} à {TimeHelper.Heure(now)}",
            MapX = 0.58 + 0.12 * (mine % 3),
            MapY = 0.46 + 0.10 * ((mine / 3) % 2),
            Activites = new()
            {
                new Activite
                {
                    Titre = "Premier signalement",
                    Detail = "Vous avez signalé la coupure.",
                    Heure = TimeHelper.Heure(now)
                }
            }
        };

        DemoData.Signalements.Insert(0, s);   // apparaît en premier sur l'accueil et la carte
        DemoData.Selected = s;
        DemoData.Brouillon = null;

        // Route absolue : efface Signaler et Récap de l'historique
        await Shell.Current.GoToAsync("//AccueilPage/" + nameof(SignalementEnvoyePage));
    }

    private async void OnEditClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");   // retour au formulaire, champs conservés
    }

    private async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}