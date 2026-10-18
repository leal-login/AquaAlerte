using AquaAlerte.Helpers;

namespace AquaAlerte.Views;

public partial class ConnexionPage : ContentPage
{
    private bool _canSubmit;

    public ConnexionPage()
    {
        InitializeComponent();
        Refresh();
    }

    // Appelé à chaque caractère tapé dans un des deux champs
    private void OnFieldChanged(object? sender, TextChangedEventArgs e) => Refresh();

    private void Refresh()
    {
        string phone = PhoneEntry.Text ?? "";
        string pwd = PasswordEntry.Text ?? "";

        bool phoneOk = FieldHelper.IsPhoneValid(phone);
        bool pwdOk = pwd.Length >= 8;

        FieldHelper.Show(PhoneBox, PhoneRow, PhoneIcon, PhoneHelper,
            phone.Length == 0, phoneOk,
            "Numéro valide : 9 chiffres après +237.",
            "Entrez 9 chiffres après +237, en commençant par 6.");

        FieldHelper.Show(PasswordBox, PasswordRow, PasswordIcon, PasswordHelper,
            pwd.Length == 0, pwdOk,
            "",
            "8 caractères minimum.");

        _canSubmit = phoneOk && pwdOk;
        FieldHelper.SetButton(LoginButton, _canSubmit);
    }

    private void OnTogglePasswordTapped(object? sender, TappedEventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        ToggleLabel.Text = PasswordEntry.IsPassword
            ? "Afficher le mot de passe"
            : "Masquer le mot de passe";
    }

    private async void OnForgotTapped(object? sender, TappedEventArgs e)
    {
        await DisplayAlertAsync("Mot de passe oublié", "Cette fonction sera ajoutée plus tard.", "OK");
    }

   private async void OnLoginClicked(object? sender, EventArgs e)
{
    if (!_canSubmit) return;
    // La vraie vérification du compte viendra plus tard.
    await Shell.Current.GoToAsync("//BienvenuePage");
}

    private async void OnCreateAccountTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(InscriptionPage));
    }
}