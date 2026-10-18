namespace AquaAlerte.Helpers;

public static class FieldHelper
{
    public static Color GetColor(string key)
    {
        if (Application.Current!.Resources.TryGetValue(key, out var value) && value is Color color)
            return color;
        return Colors.Gray;
    }

    // Numéro camerounais mobile : 9 chiffres, commençant par 6
    public static bool IsPhoneValid(string? text)
    {
        var digits = new string((text ?? "").Where(char.IsDigit).ToArray());
        return digits.Length == 9 && digits[0] == '6';
    }

    // Colore le cadre et affiche la ligne d'aide (icône + message)
    public static void Show(Border box, View row, Image icon, Label helper,
                            bool empty, bool ok, string okMsg, string errMsg)
    {
        if (empty)
        {
            box.Stroke = GetColor("AquaBorder");
            row.IsVisible = false;
            return;
        }

        var color = GetColor(ok ? "AquaSuccess" : "AquaError");
        box.Stroke = color;

        string message = ok ? okMsg : errMsg;
        helper.Text = message;
        helper.TextColor = color;
        icon.Source = ok ? "ic_ok_green.svg" : "ic_error_red.svg";
        row.IsVisible = message.Length > 0;
    }

    public static void SetButton(Button button, bool active)
    {
        button.BackgroundColor = GetColor(active ? "AquaPrimary" : "AquaDisabledBg");
        button.TextColor = active ? Colors.White : GetColor("AquaDisabledText");
    }
}