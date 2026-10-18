using AquaAlerte.Helpers;
namespace AquaAlerte.Models;

public static class DemoData
{
    public const string Synchro = "14 h 30";

        // Confirme « je constate toujours la coupure ». Renvoie false si c'est déjà fait.
    public static bool Confirmer(Signalement s)
    {
        if (s.ConfirmeParMoi || !s.EnCours) return false;

        DateTime now = DateTime.Now;
        string heure = TimeHelper.Heure(now);

        s.Confirmations++;
        s.ConfirmeParMoi = true;
        s.MiseAJour = heure;
        s.DerniereObs = $"toujours coupée à {heure}";
        s.MaConfirmation = $"{now.ToString("d MMM", TimeHelper.Fr)} à {heure}";
        s.Activites.Insert(0, new Activite
        {
            Titre = "Vous confirmez",
            Detail = "Vous constatez toujours la coupure.",
            Heure = heure
        });
        return true;
    }

    // Coupure ouverte sur l'écran de détail ou la carte
    public static Signalement? Selected { get; set; }
        // Signalement en cours de saisie (écrans « Signaler »)
    public static BrouillonSignalement? Brouillon { get; set; }

        // Alertes de l'onglet « Alertes » (données fictives)
    public static List<AlerteItem> Alertes { get; } = new()
    {
        new AlerteItem
        {
            Groupe = "Aujourd’hui", Titre = "La coupure continue à Akwa",
            Texte = "Boulevard de la Liberté : un voisin confirme que l’eau est toujours coupée.",
            Heure = "14 h 24 · il y a 6 min", Rue = "Boulevard de la Liberté"
        },
        new AlerteItem
        {
            Groupe = "Aujourd’hui", Titre = "Coupure signalée dans votre quartier",
            Texte = "Boulevard de la Liberté · début observé à 08 h 10. Consultez le signalement.",
            Heure = "08 h 10 · il y a 6 h 20", Rue = "Boulevard de la Liberté"
        },
        new AlerteItem
        {
            Groupe = "2 octobre", Titre = "Retour de l’eau signalé",
            Texte = "Rue Joss : des résidents ont observé le retour à 11 h 40.",
            Heure = "02 oct. · 11 h 45", EstRetour = true, Lue = true, Rue = "Rue Joss"
        },
    };

    // Villes et quartiers (utilisé par l'écran Signaler)
    public static Dictionary<string, List<string>> Villes { get; } = new()
    {
        ["Douala"] = new()
        {
            "Akwa", "Bessengue", "Bonanjo", "Bonapriso", "Bonabéri",
            "Deïdo", "Makèpè", "Bonamoussadi", "New Bell", "Logpom", "Kotto"
        },
        ["Yaoundé"] = new()
        {
            "Bastos", "Biyem-Assi", "Essos", "Melen", "Mvog-Mbi", "Nlongkak"
        }
    };

    public static List<Signalement> Signalements { get; } = new()
    {
        new Signalement
        {
            Quartier = "Akwa", Rue = "Boulevard de la Liberté", EnCours = true,
            Confirmations = 24, Debut = "08 h 10", DureeCourte = "6 h 20",
            MiseAJour = "14 h 24", DerniereObs = "toujours coupée à 14 h 24",
            Note = "Plus d’eau au robinet depuis ce matin, près du carrefour.",
            MapX = 0.46, MapY = 0.29,
            Activites = new()
            {
                new Activite { Titre = "Toujours coupée", Detail = "Dernière confirmation d’un voisin.", Heure = "14 h 24" },
                new Activite { Titre = "18 voisins confirment", Detail = "La coupure est encore observée.", Heure = "12 h 05" },
                new Activite { Titre = "Premier signalement", Detail = "Un voisin signale la coupure.", Heure = "08 h 10" },
            }
        },
        new Signalement
        {
            Quartier = "Bessengue", Rue = "Rue des Palmiers", EnCours = true,
            Confirmations = 8, Debut = "10 h 05", DureeCourte = "4 h 25",
            MiseAJour = "14 h 10", DerniereObs = "toujours coupée à 14 h 10",
            Note = "Le robinet est à sec depuis la matinée.",
            MapX = 0.90, MapY = 0.04,
            Activites = new()
            {
                new Activite { Titre = "Toujours coupée", Detail = "Dernière confirmation d’un voisin.", Heure = "14 h 10" },
                new Activite { Titre = "Premier signalement", Detail = "Un voisin signale la coupure.", Heure = "10 h 05" },
            }
        },
        new Signalement
        {
            Quartier = "Akwa", Rue = "Rue Joss", EnCours = false,
            Confirmations = 5, Debut = "06 h 30", Fin = "12 h 40", DureeCourte = "6 h 10",
            MiseAJour = "13 h 05", DerniereObs = "eau revenue à 12 h 40",
            Note = "Plus d’eau depuis tôt ce matin.",
            MapX = 0.24, MapY = 0.91,
            Activites = new()
            {
                new Activite { Titre = "Eau revenue", Detail = "Un voisin observe de l’eau au robinet.", Heure = "12 h 40" },
                new Activite { Titre = "Premier signalement", Detail = "Un voisin signale la coupure.", Heure = "06 h 30" },
            }
        },
    };
}