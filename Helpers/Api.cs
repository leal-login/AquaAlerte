using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AquaAlerte.Models;

namespace AquaAlerte.Helpers;

// Réponses du serveur
public record ApiUser(int Id, string Phone, string Nom, string Quartier, string Ville);
public record ApiAuth(string Token, ApiUser Utilisateur);
public record ApiSignalement(int Id, string Quartier, string Ville, string Rue, string? Note,
                             double Latitude, double Longitude, bool EnCours,
                             int Confirmations, bool ConfirmeParMoi, bool EstMien,
                             DateTime? Debut, DateTime? Fin, DateTime MiseAJour);
public record ApiAlerte(int Id, string Titre, string Texte, string Rue, bool EstRetour, DateTime Date);

// Dialogue avec le serveur AquaAlerte. Si le serveur ne répond pas, l'app reste en mode fictif.
public static class Api
{
    // Avec « adb reverse tcp:5080 tcp:5080 », le téléphone atteint le PC par localhost.
    // Par Wi-Fi, mets plutôt l'adresse du PC, par exemple "http://192.168.1.20:5080".
    private const string BaseUrl = "http://localhost:5080";

    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(4) };
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static DateTime _offlineUntil = DateTime.MinValue;

    // ===== Connexion automatique =====

    private static async Task<(HttpStatusCode Status, string Body)> Call(
        HttpMethod method, string path, object? body, string? token)
    {
        using var req = new HttpRequestMessage(method, BaseUrl + path);
        if (token != null) req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body != null)
            req.Content = new StringContent(JsonSerializer.Serialize(body, Json), Encoding.UTF8, "application/json");
        using var res = await Http.SendAsync(req);
        return (res.StatusCode, await res.Content.ReadAsStringAsync());
    }

    private static string? SaveSession(string json)
    {
        var auth = JsonSerializer.Deserialize<ApiAuth>(json, Json);
        if (auth == null) return null;
        Preferences.Default.Set("api_token", auth.Token);
        return auth.Token;
    }

    private static async Task<string?> EnsureTokenAsync()
    {
        string token = Preferences.Default.Get("api_token", "");
        if (token != "") return token;

        // Compte déjà créé sur ce téléphone : on se reconnecte
        string phone = Preferences.Default.Get("api_phone", "");
        string pass = Preferences.Default.Get("api_pass", "");
        if (phone != "" && pass != "")
        {
            var (st, txt) = await Call(HttpMethod.Post, "/api/auth/connexion", new { phone, motDePasse = pass }, null);
            if (st == HttpStatusCode.OK) return SaveSession(txt);
        }

        // Sinon (premier lancement ou serveur réinitialisé) : nouveau compte pour ce téléphone
        phone = "6" + Random.Shared.Next(0, 100_000_000).ToString("D8");
        pass = Guid.NewGuid().ToString("N");
        var (st2, txt2) = await Call(HttpMethod.Post, "/api/auth/inscription", new
        {
            phone,
            nom = "Utilisateur",
            motDePasse = pass,
            quartier = Preferences.Default.Get("quartier", "Akwa"),
            ville = Preferences.Default.Get("ville", "Douala")
        }, null);
        if (st2 != HttpStatusCode.OK) return null;

        Preferences.Default.Set("api_phone", phone);
        Preferences.Default.Set("api_pass", pass);
        return SaveSession(txt2);
    }

    // Appel avec jeton. Renvoie (false, "") si le serveur est injoignable ou refuse.
    private static async Task<(bool Ok, string Body)> Authed(HttpMethod method, string path, object? body = null)
    {
        if (DateTime.UtcNow < _offlineUntil) return (false, "");
        try
        {
            for (int attempt = 0; attempt < 2; attempt++)
            {
                string? token = await EnsureTokenAsync();
                if (token == null) return (false, "");

                var (status, text) = await Call(method, path, body, token);
                if (status == HttpStatusCode.Unauthorized)
                {
                    Preferences.Default.Remove("api_token");   // jeton périmé : on recommence
                    continue;
                }
                return (((int)status) >= 200 && ((int)status) < 300, text);
            }
            return (false, "");
        }
        catch
        {
            _offlineUntil = DateTime.UtcNow.AddSeconds(20);   // serveur éteint : on n'insiste pas
            return (false, "");
        }
    }

    // ===== Lire les données du serveur =====

    public static async Task<bool> RefreshAsync()
    {
        string ville = Preferences.Default.Get("ville", "Douala");
        string quartier = Preferences.Default.Get("quartier", "Akwa");

        var (ok, txt) = await Authed(HttpMethod.Get, $"/api/signalements?ville={Uri.EscapeDataString(ville)}");
        if (!ok) return false;

        var list = JsonSerializer.Deserialize<List<ApiSignalement>>(txt, Json);
        if (list == null) return false;

        var old = DemoData.Signalements.Where(s => s.ServerId != 0)
                                       .GroupBy(s => s.ServerId)
                                       .ToDictionary(g => g.Key, g => g.First());
        // Signalements créés par l'utilisateur hors ligne, pas encore envoyés
        var pending = DemoData.Signalements.Where(s => s.ServerId == 0 && s.EstMien).ToList();
        var selected = DemoData.Selected;

        var fresh = list.Select(a => ToLocal(a, old.TryGetValue(a.Id, out var p) ? p : null)).ToList();
        DemoData.Signalements.Clear();
        DemoData.Signalements.AddRange(fresh);
        DemoData.Signalements.AddRange(pending);

        if (selected != null && selected.ServerId != 0)
            DemoData.Selected = fresh.FirstOrDefault(s => s.ServerId == selected.ServerId) ?? selected;

        // Alertes du quartier
        var (okA, txtA) = await Authed(HttpMethod.Get,
            $"/api/alertes?quartier={Uri.EscapeDataString(quartier)}&ville={Uri.EscapeDataString(ville)}");
        if (okA)
        {
            var alertes = JsonSerializer.Deserialize<List<ApiAlerte>>(txtA, Json);
            if (alertes != null)
            {
                DemoData.Alertes.Clear();
                DemoData.Alertes.AddRange(alertes.Select(ToAlerte));
            }
        }
        return true;
    }

    // ===== Envoyer les actions de l'utilisateur =====

    // Envoie les signalements créés sur le téléphone et pas encore sur le serveur
    public static async Task<bool> PushPendingAsync()
    {
        bool any = false;
        foreach (var s in DemoData.Signalements.Where(x => x.EstMien && x.ServerId == 0).ToList())
        {
            var pair = DemoData.Villes.FirstOrDefault(v => v.Value.Contains(s.Quartier));
            string ville = string.IsNullOrEmpty(pair.Key) ? Preferences.Default.Get("ville", "Douala") : pair.Key;

            var body = new
            {
                quartier = s.Quartier,
                ville,
                rue = s.Rue,
                note = s.Note,
                debut = s.DebutConnu ? (s.DebutReel ?? ParseHeure(s.Debut))?.ToUniversalTime() : null,
                latitude = s.Latitude,
                longitude = s.Longitude
            };

            var (ok, txt) = await Authed(HttpMethod.Post, "/api/signalements", body);
            if (!ok) return any;

            var created = JsonSerializer.Deserialize<ApiSignalement>(txt, Json);
            if (created != null) { s.ServerId = created.Id; any = true; }
        }
        return any;
    }

    public static async Task<bool> ConfirmerAsync(Signalement s)
    {
        if (s.ServerId == 0) return false;
        var (ok, _) = await Authed(HttpMethod.Post, $"/api/signalements/{s.ServerId}/confirmer");
        return ok;
    }

    public static async Task<bool> RetourAsync(Signalement s)
    {
        if (s.ServerId == 0)
        {
            await PushPendingAsync();
            if (s.ServerId == 0) return false;
        }
        var (ok, _) = await Authed(HttpMethod.Post, $"/api/signalements/{s.ServerId}/retour",
                                   new { fin = ParseHeure(s.Fin)?.ToUniversalTime() });
        return ok;
    }

    // ===== Conversions =====

    // Le serveur envoie des dates UTC (parfois sans « Z ») : on les passe à l'heure du téléphone
    private static DateTime Loc(DateTime d) => DateTime.SpecifyKind(d, DateTimeKind.Utc).ToLocalTime();

    private static string Duree(DateTime from, DateTime to)
    {
        var t = to - from;
        if (t < TimeSpan.Zero) t = TimeSpan.Zero;
        int h = (int)t.TotalHours;
        return h == 0 ? $"{t.Minutes} min" : $"{h} h {t.Minutes:00}";
    }

    private static string Ago(TimeSpan t)
    {
        if (t.TotalMinutes < 1) return "à l’instant";
        if (t.TotalMinutes < 60) return $"il y a {(int)t.TotalMinutes} min";
        return $"il y a {(int)t.TotalHours} h {t.Minutes:00}";
    }

    // « 08 h 10 » -> aujourd'hui à 08:10 (hier si cette heure n'est pas encore passée)
    private static DateTime? ParseHeure(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var m = Regex.Match(text, @"(\d{1,2})\s*h\s*(\d{2})");
        if (!m.Success) return null;

        var d = DateTime.Today.AddHours(int.Parse(m.Groups[1].Value)).AddMinutes(int.Parse(m.Groups[2].Value));
        if (d > DateTime.Now.AddMinutes(2)) d = d.AddDays(-1);
        if (d > DateTime.Now) d = DateTime.Now;
        return d;
    }

    private static Signalement ToLocal(ApiSignalement a, Signalement? previous)
    {
        DateTime? debut = a.Debut.HasValue ? Loc(a.Debut.Value) : null;
        DateTime? fin = a.Fin.HasValue ? Loc(a.Fin.Value) : null;
        DateTime maj = Loc(a.MiseAJour);
        DateTime refDate = debut ?? maj;   // date affichée si le début est inconnu

        string debutTxt = debut.HasValue ? TimeHelper.Heure(debut.Value) : "";
        string finTxt = fin.HasValue ? TimeHelper.Heure(fin.Value) : "";
        string majTxt = TimeHelper.Heure(maj);

        var s = new Signalement
        {
            ServerId = a.Id,
            Quartier = a.Quartier,
            Ville = a.Ville,
            Rue = a.Rue,
            EnCours = a.EnCours,
            Confirmations = a.Confirmations,
            Note = a.Note ?? "",
            Latitude = a.Latitude,
            Longitude = a.Longitude,
            DebutConnu = debut.HasValue,
            DebutReel = debut,
            DateTexte = refDate.ToString("d MMM yyyy", TimeHelper.Fr),
            DateLongue = refDate.ToString("d MMMM", TimeHelper.Fr),
            DateComplete = refDate.ToString("d MMMM yyyy", TimeHelper.Fr),
            Debut = debutTxt,
            Fin = finTxt,
            DureeCourte = debut.HasValue ? Duree(debut.Value, fin ?? DateTime.Now) : "",
            MiseAJour = majTxt,
            HeureRef = TimeHelper.Heure(DateTime.Now),
            DerniereObs = a.EnCours ? $"toujours coupée à {majTxt}" : $"eau revenue à {finTxt}",
            Suivie = previous?.Suivie ?? false,
            ConfirmeParMoi = a.ConfirmeParMoi,
            EstMien = a.EstMien
        };

        // On garde la date exacte de ma confirmation si on la connaissait déjà
        if (a.ConfirmeParMoi)
            s.MaConfirmation = !string.IsNullOrEmpty(previous?.MaConfirmation)
                ? previous!.MaConfirmation
                : $"{maj.ToString("d MMM", TimeHelper.Fr)} à {majTxt}";

        var activites = new List<Activite>();
        if (a.ConfirmeParMoi)
            activites.Add(new Activite { Titre = "Vous confirmez", Detail = "Vous constatez toujours la coupure.", Heure = majTxt });
        if (!a.EnCours)
            activites.Add(new Activite { Titre = "Eau revenue", Detail = "Un voisin observe de l’eau au robinet.", Heure = finTxt });
        else if (a.Confirmations > 1)
            activites.Add(new Activite { Titre = "Toujours coupée", Detail = "Dernière confirmation d’un voisin.", Heure = majTxt });
        activites.Add(new Activite
        {
            Titre = "Premier signalement",
            Detail = "Un voisin signale la coupure.",
            Heure = debut.HasValue ? debutTxt : "—"
        });
        s.Activites = activites;

        return s;
    }

    private static AlerteItem ToAlerte(ApiAlerte a)
    {
        DateTime d = Loc(a.Date);
        bool today = d.Date == DateTime.Today;

        return new AlerteItem
        {
            Groupe = today ? "Aujourd’hui" : d.ToString("d MMMM", TimeHelper.Fr),
            Titre = a.Titre,
            Texte = a.Texte,
            Heure = today
                ? $"{TimeHelper.Heure(d)} · {Ago(DateTime.Now - d)}"
                : $"{d.ToString("dd MMM", TimeHelper.Fr)} · {TimeHelper.Heure(d)}",
            Rue = a.Rue,
            EstRetour = a.EstRetour,
            Lue = !today
        };
    }
}