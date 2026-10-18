using AquaAlerte.Views;

namespace AquaAlerte;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(ConnexionPage), typeof(ConnexionPage));
        Routing.RegisterRoute(nameof(InscriptionPage), typeof(InscriptionPage));
        Routing.RegisterRoute(nameof(QuartierPage), typeof(QuartierPage));
        Routing.RegisterRoute(nameof(SignalerPage), typeof(SignalerPage));
        Routing.RegisterRoute(nameof(DetailCoupurePage), typeof(DetailCoupurePage));
    }
}