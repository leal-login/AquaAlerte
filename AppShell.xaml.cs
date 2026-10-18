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
        Routing.RegisterRoute(nameof(RecapSignalementPage), typeof(RecapSignalementPage));
        Routing.RegisterRoute(nameof(SignalementEnvoyePage), typeof(SignalementEnvoyePage));
        Routing.RegisterRoute(nameof(DetailCoupurePage), typeof(DetailCoupurePage));
        Routing.RegisterRoute(nameof(ObservationRecuePage), typeof(ObservationRecuePage));
        Routing.RegisterRoute(nameof(RetourEauPage), typeof(RetourEauPage));
    }
}