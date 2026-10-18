namespace AquaAlerte.Views;

public partial class ProfilPage : ContentPage
{
    public ProfilPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        string quartier = Preferences.Default.Get("quartier", "Akwa");
        string ville = Preferences.Default.Get("ville", "Douala");

        NameLabel.Text = VoisinDe(quartier);
        QuartierLabel.Text = quartier;
        VilleLabel.Text = $"{ville} · Cameroun";
        NewDescLabel.Text = $"Être informé des signalements à {quartier}.";

        // Réglages mémorisés sur le téléphone (par défaut : les deux premiers activés)
        SwitchNew.IsToggled = Preferences.Default.Get("pref_nouvelles", true);
        SwitchReturn.IsToggled = Preferences.Default.Get("pref_retour", true);
        SwitchConf.IsToggled = Preferences.Default.Get("pref_confirmations", false);

        ShowLang(Preferences.Default.Get("langue", "fr"));
    }

    // « Voisin d'Akwa », « Voisin de Bonanjo »
    private static string VoisinDe(string quartier)
    {
        string n = quartier.Trim();
        if (n.Length == 0) return "Voisin";

        bool voyelle = "aeiouyàâäéèêëîïôöûü".IndexOf(char.ToLowerInvariant(n[0])) >= 0;
        return voyelle ? $"Voisin d’{n}" : $"Voisin de {n}";
    }

    // Chaque interrupteur enregistre son choix sous le nom indiqué dans AutomationId
    private void OnPrefToggled(object? sender, ToggledEventArgs e)
    {
        if (sender is Switch { AutomationId: { Length: > 0 } key })
            Preferences.Default.Set(key, e.Value);
    }

    private void OnLangTapped(object? sender, TappedEventArgs e)
    {
        string lang = e.Parameter as string ?? "fr";
        Preferences.Default.Set("langue", lang);
        ShowLang(lang);
    }

    private void ShowLang(string lang)
    {
        LangFrIcon.Source = lang == "fr" ? "ic_check_circle_teal.svg" : "ic_radio_empty.svg";
        LangEnIcon.Source = lang == "en" ? "ic_check_circle_teal.svg" : "ic_radio_empty.svg";
    }

    private async void OnChangeQuartierTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(QuartierPage));
    }
}