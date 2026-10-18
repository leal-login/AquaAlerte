using System.Globalization;
using System.Text;
using AquaAlerte.Helpers;
using AquaAlerte.Models;

namespace AquaAlerte.Views;

public partial class QuartierPage : ContentPage
{
    // Villes et quartiers (données fictives, modifiables)
    private readonly Dictionary<string, List<string>> _data = new()
    {
        ["Douala"] = new()
        {
            "Akwa", "Bessengue", "Bonanjo", "Bonapriso", "Bonabéri",
            "Deïdo", "Makèpè", "Bonamoussadi", "New Bell", "Logpom", "Kotto"
        },
        ["Yaoundé"] = new()
        {
            "Bastos", "Biyem-Assi", "Essos", "Melen", "Mvog-Mbi", "Nlongkak"
        }
    };

    private List<QuartierItem> _all = new();   // quartiers de la ville choisie
    private QuartierItem? _selected;           // quartier choisi (null = aucun)

    public QuartierPage()
    {
        InitializeComponent();
        CityPicker.ItemsSource = _data.Keys.ToList();
        CityPicker.SelectedIndex = 0;          // déclenche OnCityChanged
    }

    private void OnCityChanged(object? sender, EventArgs e)
    {
        if (CityPicker.SelectedItem is not string city) return;

        _selected = null;
        _all = _data[city].Select(n => new QuartierItem { Name = n, Ville = city }).ToList();
        SearchEntry.Text = "";
        ApplyFilter();
        UpdateButton();
    }

    private void OnSearchChanged(object? sender, TextChangedEventArgs e) => ApplyFilter();

    // Affiche seulement les quartiers qui contiennent le texte cherché
    private void ApplyFilter()
    {
        string query = Normalize((SearchEntry.Text ?? "").Trim());
        var list = _all.Where(q => Normalize(q.Name).Contains(query)).ToList();

        BindableLayout.SetItemsSource(QuartierLayout, list);
        EmptyLabel.IsVisible = list.Count == 0;
    }

    // Ignore les accents et les majuscules : « bonabéri » = « Bonaberi »
    private static string Normalize(string text)
    {
        var sb = new StringBuilder();
        foreach (char c in text.Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        return sb.ToString().ToLowerInvariant();
    }

    private void OnItemTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Border { BindingContext: QuartierItem item }) return;

        if (_selected != null) _selected.IsSelected = false;
        item.IsSelected = true;
        _selected = item;
        UpdateButton();
    }

    private void UpdateButton()
    {
        ContinueButton.Text = _selected == null ? "Continuer" : $"Continuer avec {_selected.Name}";
        FieldHelper.SetButton(ContinueButton, _selected != null);
    }

    private async void OnUsePositionClicked(object? sender, EventArgs e)
    {
        await DisplayAlertAsync("Position",
            "Cette fonction sera ajoutée plus tard. Choisissez votre quartier dans la liste.", "OK");
    }

    private async void OnContinueClicked(object? sender, EventArgs e)
{
    if (_selected == null) return;

    // On mémorise le choix pour l'accueil et la carte
    Preferences.Default.Set("quartier", _selected.Name);
    Preferences.Default.Set("ville", _selected.Ville);

    await Shell.Current.GoToAsync("//AccueilPage");
}

    private async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}