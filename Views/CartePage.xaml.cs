using Microsoft.Maui.Layouts;
using AquaAlerte.Helpers;
using AquaAlerte.Models;
using Microsoft.Maui.Controls.Shapes;

namespace AquaAlerte.Views;

public partial class CartePage : ContentPage
{
    private string _filter = "all";                 // all, encours ou revenue
    private Signalement? _selected;                 // coupure affichée dans la fiche
    private readonly List<View> _markers = new();   // repères ajoutés sur la carte

    public CartePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        string quartier = Preferences.Default.Get("quartier", "Akwa");
        string ville = Preferences.Default.Get("ville", "Douala");
        QuartierLabel.Text = quartier;
        VilleLabel.Text = $"{ville} · Cameroun";

        _selected = DemoData.Selected
            ?? DemoData.Signalements.FirstOrDefault(s => s.EnCours && s.Quartier == quartier)
            ?? DemoData.Signalements.FirstOrDefault();

        SetFilter("all");
        ShowDetail(_selected);
    }

    // ----- Filtres -----

    private void OnChipTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is string filter) SetFilter(filter);
    }

    private void SetFilter(string filter)
    {
        _filter = filter;
        StyleChip(ChipAll, ChipAllLabel, filter == "all");
        StyleChip(ChipOngoing, ChipOngoingLabel, filter == "encours");
        StyleChip(ChipRestored, ChipRestoredLabel, filter == "revenue");
        BuildMarkers();
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

    // ----- Repères sur la carte -----

    private void BuildMarkers()
    {
        foreach (var m in _markers) MapLayer.Children.Remove(m);
        _markers.Clear();

        foreach (var s in DemoData.Signalements.Where(PassesFilter))
        {
            var marker = new Border
            {
                WidthRequest = 44,
                HeightRequest = 44,
                StrokeShape = new Ellipse(),
                Stroke = Colors.White,
                StrokeThickness = 3,
                BackgroundColor = s.Accent,
                Padding = 0,
                BindingContext = s,
                Scale = s == _selected ? 1.2 : 1.0,
                Content = new Image
                {
                    Source = s.EnCours ? "ic_water_off_white.svg" : "ic_check_white.svg",
                    WidthRequest = 22,
                    HeightRequest = 22,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += OnMarkerTapped;
            marker.GestureRecognizers.Add(tap);

            AbsoluteLayout.SetLayoutBounds(marker, new Rect(s.MapX, s.MapY, 44, 44));
            AbsoluteLayout.SetLayoutFlags(marker, AbsoluteLayoutFlags.PositionProportional);

            MapLayer.Children.Add(marker);
            _markers.Add(marker);
        }
    }

    private void OnMarkerTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border { BindingContext: Signalement s })
        {
            _selected = s;
            DemoData.Selected = s;
            BuildMarkers();      // met en évidence le repère choisi
            ShowDetail(s);
        }
    }

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
            ? $"{s.Quartier} · début observé à {s.Debut}"
            : $"{s.Quartier} · eau revenue à {s.Fin}";
        SheetSince.Text = s.DepuisTexte;
        ConfirmButton.IsVisible = s.EnCours;
    }

    private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        if (_selected == null) return;
        _selected.Confirmations++;
        ShowDetail(_selected);
        await DisplayAlertAsync("Merci", "Votre confirmation est prise en compte (démonstration).", "OK");
    }

    private async void OnChangeQuartierTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(QuartierPage));
    }

    private async void OnSeeDetailTapped(object? sender, TappedEventArgs e)
    {
    if (_selected == null) return;
    DemoData.Selected = _selected;
    await Shell.Current.GoToAsync(nameof(DetailCoupurePage));
    }
}