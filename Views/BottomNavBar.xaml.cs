using AquaAlerte.Helpers;

namespace AquaAlerte.Views;

public partial class BottomNavBar : ContentView
{
    // Propriété qu'on renseigne dans chaque page : Active="AccueilPage"
    public static readonly BindableProperty ActiveProperty = BindableProperty.Create(
        nameof(Active), typeof(string), typeof(BottomNavBar), "",
        propertyChanged: (bindable, _, _) => ((BottomNavBar)bindable).Update());

    public string Active
    {
        get => (string)GetValue(ActiveProperty);
        set => SetValue(ActiveProperty, value);
    }

    private readonly List<(string Route, string Key, Image Icon, Label Label, BoxView Bar)> _tabs;

    public BottomNavBar()
    {
        InitializeComponent();
        _tabs = new()
        {
            ("AccueilPage",  "home", HomeIcon, HomeLabel, HomeBar),
            ("CartePage",    "map",  MapIcon,  MapLabel,  MapBar),
            ("SignalerPage", "plus", PlusIcon, PlusLabel, PlusBar),
            ("AlertesPage",  "bell", BellIcon, BellLabel, BellBar),
            ("ProfilPage",   "user", UserIcon, UserLabel, UserBar),
        };
        Update();
    }

    // Onglet actif : icône et texte bleus, trait dessous. Les autres : gris.
    private void Update()
    {
        if (_tabs == null) return;
        foreach (var t in _tabs)
        {
            bool on = t.Route == Active;
            t.Icon.Source = $"nav_{t.Key}_{(on ? "on" : "off")}.svg";
            t.Label.TextColor = FieldHelper.GetColor(on ? "AquaPrimary" : "AquaMuted");
            t.Label.FontAttributes = on ? FontAttributes.Bold : FontAttributes.None;
            t.Bar.IsVisible = on;
        }
    }

   private async void OnTabTapped(object? sender, TappedEventArgs e)
{
    if (e.Parameter is not string route || route == Active) return;

    if (route == "SignalerPage")
        await Shell.Current.GoToAsync(nameof(SignalerPage));   // s'ouvre par-dessus
    else
        await Shell.Current.GoToAsync("//" + route);
}
}