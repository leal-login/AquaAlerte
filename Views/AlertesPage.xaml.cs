using AquaAlerte.Helpers;
using AquaAlerte.Models;

namespace AquaAlerte.Views;

public partial class AlertesPage : ContentPage
{
    private bool _onlyUnread;   // false = « Toutes », true = « Non lues »

    public AlertesPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SubLabel.Text = $"Observations de la communauté · {Preferences.Default.Get("quartier", "Akwa")}";
        Refresh();
    }

    // Recalcule tout : compteur, filtres et liste
    private void Refresh()
    {
        StyleChip(ChipAll, ChipAllLabel, !_onlyUnread);
        StyleChip(ChipUnread, ChipUnreadLabel, _onlyUnread);

        int unread = DemoData.Alertes.Count(a => a.NonLue);
        UnreadLabel.Text = unread switch
        {
            0 => "Tout est lu",
            1 => "1 non lue",
            _ => $"{unread} non lues"
        };
        MarkAllLabel.IsVisible = unread > 0;

        var groups = DemoData.Alertes
            .Where(a => !_onlyUnread || a.NonLue)
            .GroupBy(a => a.Groupe)
            .Select(g => new AlerteGroupe { Titre = g.Key, Items = g.ToList() })
            .ToList();

        BindableLayout.SetItemsSource(GroupsLayout, groups);
        EmptyLabel.IsVisible = groups.Count == 0;
    }

    private static void StyleChip(Border chip, Label label, bool active)
    {
        chip.BackgroundColor = active ? FieldHelper.GetColor("AquaPrimary") : Colors.White;
        chip.Stroke = active ? FieldHelper.GetColor("AquaPrimary") : FieldHelper.GetColor("AquaBorder");
        label.TextColor = active ? Colors.White : FieldHelper.GetColor("AquaMuted");
    }

    private void OnChipTapped(object? sender, TappedEventArgs e)
    {
        _onlyUnread = e.Parameter as string == "unread";
        Refresh();
    }

    private void OnMarkAllTapped(object? sender, TappedEventArgs e)
    {
        foreach (var a in DemoData.Alertes) a.Lue = true;
        Refresh();
    }

    // Toucher une alerte la marque comme lue et ouvre la coupure liée
    private async void OnAlertTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Border { BindingContext: AlerteItem item }) return;

        item.Lue = true;

        var s = DemoData.Signalements.FirstOrDefault(x => x.Rue == item.Rue);
        if (s == null)
        {
            Refresh();
            return;
        }

        DemoData.Selected = s;
        await Shell.Current.GoToAsync(nameof(DetailCoupurePage));   // Refresh() se fera au retour
    }
}