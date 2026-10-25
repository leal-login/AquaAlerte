using System.Globalization;
using System.Text.Json;
using AquaAlerte.Helpers;
using AquaAlerte.Models;

namespace AquaAlerte.Views;

public partial class CartePage : ContentPage
{
    // Adresse « d'origine » de la page de la carte. OpenStreetMap exige une adresse
    // qui identifie l'application quand elle télécharge ses tuiles (le Referer).
    private const string MapBaseUrl = "https://github.com/leal-login/AquaAlerte";

    private string _filter = "all";                  // all, encours ou revenue
    private Signalement? _selected;                  // coupure affichée dans la fiche
    private List<Signalement> _shown = new();        // coupures sur la carte (identifiant = position dans cette liste)
    private bool _htmlLoaded;                        // la page de la carte a été donnée au WebView
    private bool _mapReady;                          // la page est chargée, on peut lui envoyer des repères

    public CartePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        string quartier = Preferences.Default.Get("quartier", "Akwa");
        string ville = Preferences.Default.Get("ville", "Douala");
        QuartierLabel.Text = quartier;
        VilleLabel.Text = $"{ville} · Cameroun";
        await Api.RefreshAsync();   // charge les données du serveur (sans effet si le serveur est éteint)

        _selected = DemoData.Selected
            ?? DemoData.Signalements.FirstOrDefault(s => s.EnCours && s.Quartier == quartier)
            ?? DemoData.Signalements.FirstOrDefault();

        _filter = "all";
        StyleChips();
        ShowDetail(_selected);

        if (!_htmlLoaded)
        {
            _htmlLoaded = true;
            MapView.Source = BuildSource();   // OnMapNavigated enverra les repères une fois la carte prête
        }
        else
        {
            await PushMarkers(DemoData.Selected != null ? "selected" : "all");
        }
    }

    private static HtmlWebViewSource BuildSource() =>
        new() { Html = MapHtml.Build(), BaseUrl = MapBaseUrl };

    // ----- Dialogue entre la carte (JavaScript) et le C# -----

    // La carte envoie un message en changeant son adresse : aqua://marker/2, aqua://locate...
    private async void OnMapNavigating(object? sender, WebNavigatingEventArgs e)
    {
        string url = e.Url ?? "";

        if (url.StartsWith("aqua://", StringComparison.OrdinalIgnoreCase))
        {
            e.Cancel = true;   // on ne quitte pas la page de la carte
            await HandleMapMessage(url["aqua://".Length..].Trim('/'));
            return;
        }

        // Un lien de la carte (ex. « OpenStreetMap ») s'ouvre dans le navigateur, pas dans la carte
        if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            && !url.StartsWith(MapBaseUrl, StringComparison.OrdinalIgnoreCase))
        {
            e.Cancel = true;
            try { await Launcher.OpenAsync(url); } catch { /* pas de navigateur : on ignore */ }
        }
    }

    // La page de la carte est chargée : on peut y placer les repères
    private async void OnMapNavigated(object? sender, WebNavigatedEventArgs e)
    {
        if (_mapReady || e.Result != WebNavigationResult.Success) return;
        if ((e.Url ?? "").StartsWith("aqua://", StringComparison.OrdinalIgnoreCase)) return;

        _mapReady = true;
        await PushMarkers(DemoData.Selected != null ? "selected" : "all");
    }

    private async Task HandleMapMessage(string message)
    {
        if (message.StartsWith("marker/") && int.TryParse(message["marker/".Length..], out int id)
            && id >= 0 && id < _shown.Count)
        {
            _selected = _shown[id];
            DemoData.Selected = _selected;
            ShowDetail(_selected);
        }
        else if (message == "locate")
        {
            await LocateAsync();
        }
        else if (message == "reload")
        {
            _mapReady = false;
            MapView.Source = BuildSource();
        }
    }

    // Envoie une commande JavaScript à la carte
    private async Task RunJs(string script)
    {
        try { await MapView.EvaluateJavaScriptAsync(script); }
        catch { /* la carte n'est pas prête : on ignore */ }
    }

    // Place les repères sur la carte. mode : keep, all ou selected
    private async Task PushMarkers(string mode)
    {
        if (!_mapReady) return;

        _shown = DemoData.Signalements
            .Where(s => PassesFilter(s) && (s.Latitude != 0 || s.Longitude != 0))
            .ToList();
        int selId = _selected == null ? -1 : _shown.IndexOf(_selected);

        var items = _shown.Select((s, i) => new { id = i, lat = s.Latitude, lng = s.Longitude, ongoing = s.EnCours });
        string json = JsonSerializer.Serialize(items);   // les nombres gardent le point décimal

        await RunJs($"setMarkers({json},{selId},'{mode}')");
    }

    // ----- Ma position (point bleu « Vous ») -----

    private async Task LocateAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                await DisplayAlertAsync("Position",
                    "La localisation est refusée. Vous pouvez l’autoriser dans les réglages de l’appareil.", "OK");
                return;
            }

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            Location? loc = await Geolocation.Default.GetLocationAsync(request);
            if (loc == null)
            {
                await DisplayAlertAsync("Position", "Votre position est introuvable pour le moment.", "OK");
                return;
            }

            // Si l'utilisateur est loin de la ville affichée, on n'affiche pas son point
            string ville = Preferences.Default.Get("ville", "Douala");
            var centre = DemoData.CentreVille(ville);
            double km = Location.CalculateDistance(loc.Latitude, loc.Longitude,
                                                   centre.Lat, centre.Lng, DistanceUnits.Kilometers);
            if (km > 60)
            {
                await DisplayAlertAsync("Position",
                    $"Vous semblez être loin de {ville}. Votre position n’est pas affichée sur cette carte.", "OK");
                return;
            }

            string lat = loc.Latitude.ToString(CultureInfo.InvariantCulture);
            string lng = loc.Longitude.ToString(CultureInfo.InvariantCulture);
            await RunJs($"setUser({lat},{lng})");
        }
        catch (FeatureNotEnabledException)
        {
            await DisplayAlertAsync("Position", "Activez la localisation de votre téléphone, puis réessayez.", "OK");
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Position", "Impossible de lire votre position pour le moment.", "OK");
        }
    }

    // ----- Filtres -----

    private async void OnChipTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not string filter) return;
        _filter = filter;
        StyleChips();
        await PushMarkers("keep");   // la carte garde son zoom et sa position
    }

    private void StyleChips()
    {
        StyleChip(ChipAll, ChipAllLabel, _filter == "all");
        StyleChip(ChipOngoing, ChipOngoingLabel, _filter == "encours");
        StyleChip(ChipRestored, ChipRestoredLabel, _filter == "revenue");
    }

    private static void StyleChip(Border chip, Label label, bool active)
    {
        chip.BackgroundColor = active ? FieldHelper.GetColor("AquaPrimary") : Colors.White;
        chip.Stroke = active ? FieldHelper.GetColor("AquaPrimary") : FieldHelper.GetColor("AquaBorder");
        label.TextColor = active ? Colors.White : FieldHelper.GetColor("AquaMuted");
    }

    private bool PassesFilter(Signalement s) => _filter switch
    {
        "encours" => s.EnCours,
        "revenue" => !s.EnCours,
        _ => true
    };

    // ----- Fiche du bas -----

    private void ShowDetail(Signalement? s)
    {
        SheetContent.IsVisible = s != null;
        EmptySheetLabel.IsVisible = s == null;
        if (s == null) return;

        SheetBadgeBox.BackgroundColor = s.BadgeBg;
        SheetBadgeIcon.Source = s.BadgeIcon;
        SheetBadgeLabel.Text = s.StatutTexte;
        SheetBadgeLabel.TextColor = s.Accent;
        SheetConfLabel.Text = s.ConfirmationsTexte;
        SheetTitle.Text = s.Rue;
        SheetSub.Text = s.EnCours
            ? (s.DebutConnu ? $"{s.Quartier} · début observé à {s.Debut}" : $"{s.Quartier} · début inconnu")
            : $"{s.Quartier} · eau revenue à {s.Fin}";
        SheetSince.Text = s.DepuisTexte;
        ConfirmButton.IsVisible = s.EnCours;
    }

       private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        if (_selected == null) return;

        if (!DemoData.Confirmer(_selected))
        {
            await DisplayAlertAsync("Déjà confirmée", "Vous avez déjà confirmé cette coupure.", "OK");
            return;
        }

        await Api.ConfirmerAsync(_selected);   // informe le serveur (ignoré s'il est éteint)

        DemoData.Selected = _selected;
        ShowDetail(_selected);
        await Shell.Current.GoToAsync(nameof(ObservationRecuePage));
    }
    private async void OnSeeDetailTapped(object? sender, TappedEventArgs e)
    {
        if (_selected == null) return;
        DemoData.Selected = _selected;
        await Shell.Current.GoToAsync(nameof(DetailCoupurePage));
    }

    private async void OnChangeQuartierTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(QuartierPage));
    }
}