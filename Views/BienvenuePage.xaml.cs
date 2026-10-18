namespace AquaAlerte.Views;

public partial class BienvenuePage : ContentPage
{
    public BienvenuePage()
    {
        InitializeComponent();
    }

    private async void OnChooseClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(QuartierPage));
    }
}