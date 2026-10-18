using AquaAlerte.Helpers;
namespace AquaAlerte.Models;

public class Activite
{
    public string Titre { get; set; } = "";
    public string Detail { get; set; } = "";
    public string Heure { get; set; } = "";
}

public class Signalement
{
    public string Quartier { get; set; } = "";
    public string Ville { get; set; } = "Douala";
    public string Rue { get; set; } = "";
    public bool EnCours { get; set; } = true;
    public int Confirmations { get; set; }
    public bool DebutConnu { get; set; } = true;
    public string DateTexte { get; set; } = "6 oct. 2026";
    public string DateLongue { get; set; } = "6 octobre";
    public string Debut { get; set; } = "";          // ex. "08 h 10"
    public string DureeCourte { get; set; } = "";    // ex. "6 h 20"
    public string Fin { get; set; } = "";            // pour « eau revenue »
    public string MiseAJour { get; set; } = "";      // ex. "14 h 24"
    public string HeureRef { get; set; } = DemoData.Synchro;   // heure de référence de la durée
    public string DerniereObs { get; set; } = "";    // ex. "toujours coupée à 14 h 24"
    public string Note { get; set; } = "";
    public bool Suivie { get; set; }                 // l'utilisateur suit cette coupure
    public bool EstMien { get; set; } 
    public string DateComplete { get; set; } = "6 octobre 2026";
    public DateTime? DebutReel { get; set; }          // début exact (signalements créés dans l'app)
    public bool ConfirmeParMoi { get; set; }          // l'utilisateur a déjà confirmé
    public string MaConfirmation { get; set; } = "";  // ex. « 6 oct. à 14 h 30 »

    // Durée entre le début et une heure de fin ; null si inconnue ou incohérente
    public TimeSpan? DureeJusqua(DateTime fin)
    {
        if (!DebutConnu) return null;

        TimeSpan d;
        if (DebutReel.HasValue) d = fin - DebutReel.Value;
        else if (TimeHelper.TryHeure(Debut, out var h)) d = fin.TimeOfDay - h;
        else return null;

        return d < TimeSpan.Zero ? null : d;
    }               // signalement créé par l'utilisateur
    public List<Activite> Activites { get; set; } = new();

    // Position du repère sur la carte (0 = bord gauche/haut, 1 = bord droit/bas)
    public double MapX { get; set; }
    public double MapY { get; set; }

    // ----- Textes pour l'accueil et la carte -----
    public string Titre => $"{Quartier} · {Rue}";
    public string StatutTexte => EnCours ? "En cours" : "Eau revenue";
    public string ConfirmationsTexte => Confirmations == 1 ? "1 confirmation" : $"{Confirmations} confirmations";
    public string DepuisTexte => EnCours
        ? (DebutConnu ? $"Depuis {Debut} · {DureeCourte} observées" : "Début inconnu · durée inconnue")
        : $"Retour de l’eau à {Fin}";
    public string MajTexte => $"Communauté · mise à jour {MiseAJour}";
    public Color Accent => Color.FromArgb(EnCours ? "#D64545" : "#1E9E6A");
    public Color BadgeBg => Color.FromArgb(EnCours ? "#FDECEC" : "#E4F5EC");
    public string BadgeIcon => EnCours ? "ic_error_red.svg" : "ic_ok_green.svg";

    // ----- Textes pour l'écran de détail -----
    public string SousTitre => $"{Quartier} · {Ville}";
    public string DureeAffichee => DebutConnu ? DureeCourte : "Inconnue";
    public string DureeLabel => EnCours ? $"DURÉE OBSERVÉE À {HeureRef.ToUpper()}" : "DURÉE DE LA COUPURE";
    public string DebutTexte => DebutConnu ? $"Début signalé : {DateTexte} à {Debut}" : "Début non renseigné";
    public string DureeNote => EnCours
        ? "Durée depuis le début rapporté, pas un délai de rétablissement."
        : "Durée entre le début rapporté et le retour de l’eau signalé par un voisin.";
    public string ConfirmationsDetail => Confirmations == 1
        ? "1 confirmation de voisin" : $"{Confirmations} confirmations de voisins";
    public string DerniereObsTexte => $"Dernière observation : {DerniereObs}";
    public string InfoTexte => EnCours
        ? "Aucune heure de retour connue. Les informations sont issues de la communauté."
        : $"Retour de l’eau signalé à {Fin}. Les informations sont issues de la communauté.";
    public bool HasNote => !string.IsNullOrWhiteSpace(Note);
    public string NoteTexte => $"« {Note} »";
}