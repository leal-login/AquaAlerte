namespace AquaAlerte.Models;

public class BrouillonSignalement
{
    public string Quartier { get; set; } = "";
    public string Ville { get; set; } = "";
    public string Rue { get; set; } = "";
    public string Repere { get; set; } = "";
    public string Observation { get; set; } = "";
    public DateTime? Debut { get; set; }     // null = début inconnu

    public string RueOuDefaut => string.IsNullOrWhiteSpace(Rue) ? "Secteur non précisé" : Rue;
    public string Titre => $"{Quartier} · {RueOuDefaut}";
}