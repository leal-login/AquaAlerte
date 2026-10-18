using AquaAlerte.Helpers;

namespace AquaAlerte.Views;

public partial class InscriptionPage : ContentPage
{
    private bool _canSubmit;

    public InscriptionPage()
    {
        InitializeComponent();
        Refresh();
    }

    private void OnFieldChanged(object? sender, TextChangedEventArgs e) => Refresh();

    private void Refresh()
    {
        string name = (NameEntry.Text ?? "").Trim();
        string phone = PhoneEntry.Text ?? "";
        string pwd = PasswordEntry.Text ?? "";
        string confirm = ConfirmEntry.Text ?? "";

        bool nameOk = name.Length >= 2;
        bool phoneOk = FieldHelper.IsPhoneValid(phone);
        bool pwdOk = pwd.Length >= 8;
        bool confirmOk = confirm.Length > 0 && confirm == pwd;

        // Nom : pas de message, juste un cadre neutre
        NameBox.Stroke = FieldHelper.GetColor("AquaBorder");

        FieldHelper.Show(PhoneBox, PhoneRow, PhoneIcon, PhoneHelper,
            phone.Length == 0, phoneOk,
            "Numéro valide : 9 chiffres après +237.",
            "Entrez 9 chiffres après +237, en commençant par 6.");

        FieldHelper.Show(PasswordBox, PasswordRow, PasswordIcon, PasswordHelper,
            pwd.Length == 0, pwdOk,
            "8 caractères minimum : règle respectée.",
            "8 caractères minimum.");

        FieldHelper.Show(ConfirmBox, ConfirmRow, ConfirmIcon, ConfirmHelper,
            confirm.Length == 0, confirmOk,
            "Les mots de passe correspondent.",
            "Les mots de passe ne correspondent pas.");

        _canSubmit = nameOk && phoneOk && pwdOk && confirmOk;
        FieldHelper.SetButton(SignUpButton, _canSubmit);
    }

    private void OnTogglePasswordTapped(object? sender, TappedEventArgs e)
    {
        bool hide = !PasswordEntry.IsPassword;
        PasswordEntry.IsPassword = hide;
        ConfirmEntry.IsPassword = hide;
        ToggleLabel.Text = hide ? "Afficher le mot de passe" : "Masquer le mot de passe";
    }

  private async void OnSignUpClicked(object? sender, EventArgs e)
{
    if (!_canSubmit) return;
    // La vraie création du compte viendra plus tard.
    await Shell.Current.GoToAsync("//BienvenuePage");
}

    private async void OnBackToLoginTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}