namespace AquaAlerte.Views;

public partial class OnboardingPage : ContentPage
{
    private const int Total = 3;   // nombre d'écrans
    private int _index = 0;        // écran affiché : 0, 1 ou 2

    public OnboardingPage()
    {
        InitializeComponent();
        ShowSlide(0);
    }

    // Appelé quand on appuie sur « Suivant » (ou « Choisir mon quartier »)
    private void OnNextClicked(object? sender, EventArgs e)
    {
        if (_index < Total - 1)
            ShowSlide(_index + 1);
        else
            Terminer();
    }

    // Appelé quand on appuie sur « Passer »
    private void OnSkipTapped(object? sender, TappedEventArgs e)
    {
        Terminer();
    }

    // Affiche l'écran demandé et met à jour tout ce qui dépend de lui
    private void ShowSlide(int index)
    {
        _index = index;

        Slide1.IsVisible = index == 0;
        Slide2.IsVisible = index == 1;
        Slide3.IsVisible = index == 2;

        SetDot(Dot1, index == 0);
        SetDot(Dot2, index == 1);
        SetDot(Dot3, index == 2);
        CounterLabel.Text = $"{index + 1} sur {Total}";

        bool dernier = index == Total - 1;
        NextButton.Text = dernier ? "Choisir mon quartier" : "Suivant";
        SkipLabel.IsVisible = !dernier;
    }

    // Pastille active : allongée et bleue. Inactive : courte et grise.
    private void SetDot(BoxView dot, bool active)
    {
        dot.WidthRequest = active ? 28 : 8;
        dot.Color = GetColor(active ? "AquaPrimary" : "AquaBorder");
    }

    // Lit une couleur déclarée dans AquaStyles.xaml
    private static Color GetColor(string key)
    {
        if (Application.Current!.Resources.TryGetValue(key, out var value) && value is Color color)
            return color;
        return Colors.Gray;
    }

    private async void Terminer()
{
    // Provisoire : on envoie vers la connexion pour tester le parcours.
    // Plus tard, le dernier bouton mènera à « Choisir mon quartier ».
    await Shell.Current.GoToAsync(nameof(ConnexionPage));
}
}