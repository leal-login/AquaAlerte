namespace AquaAlerte.Models;

public class AlerteItem
{
    public string Groupe { get; set; } = "";   // « Aujourd’hui », « 2 octobre »...
    public string Titre { get; set; } = "";
    public string Texte { get; set; } = "";
    public string Heure { get; set; } = "";    // ex. « 14 h 24 · il y a 6 min »
    public bool EstRetour { get; set; }        // true = retour de l'eau, false = coupure
    public bool Lue { get; set; }
    public string Rue { get; set; } = "";      // sert à retrouver la coupure liée

    public bool NonLue => !Lue;
    public string IconeSource => EstRetour ? "ic_drop_green.svg" : "ic_warning_red.svg";
    public Color IconeFond => Color.FromArgb(EstRetour ? "#E4F5EC" : "#FDECEC");
    public Color CarteFond => Lue ? Colors.White : Color.FromArgb("#E6F5FA");
    public Brush CarteBord => new SolidColorBrush(Color.FromArgb(Lue ? "#D9E1E8" : "#BFE3F0"));
}

public class AlerteGroupe
{
    public string Titre { get; set; } = "";
    public List<AlerteItem> Items { get; set; } = new();
}