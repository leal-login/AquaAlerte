# AquaAlerte

Application mobile **.NET MAUI (Android)** pour signaler et suivre les coupures d'eau dans son quartier.

> **Données fictives.** Les informations affichées dans l'application sont des données de démonstration. Les signalements sont des observations citoyennes, pas une information officielle du fournisseur d'eau.

## Présentation

AquaAlerte permet aux habitants d'un quartier (par exemple Akwa, à Douala) de signaler une coupure d'eau, de confirmer l'observation d'un voisin, de signaler le retour de l'eau et de suivre l'évolution de chaque coupure. Aucun compte n'est requis pour consulter les coupures.

## Fonctionnalités

- **Onboarding** en 3 écrans (une seule page, trois blocs), puis écran de bienvenue
- **Connexion** et **création de compte** (téléphone et mot de passe)
- **Choix de la ville et du quartier**, mémorisé sur le téléphone
- **Accueil du quartier** : compteurs, coupures en cours, coupures à proximité
- **Carte schématique** avec filtres (Toutes, En cours, Eau revenue) et fiche de la coupure sélectionnée
- **Détail d'une coupure** : durée observée, confirmations de voisins, note, activité de la communauté
- **Signalement en 2 étapes** (formulaire, puis récapitulatif) et écran de confirmation d'envoi. La nouvelle coupure apparaît aussitôt sur l'accueil, la carte et le détail
- **Confirmation** « Je constate toujours la coupure » (une confirmation par personne)
- **Retour de l'eau** : heure choisie, note facultative, statut mis à jour
- **Alertes** du quartier groupées par date, filtre « Non lues », « Tout marquer comme lu »
- **Profil** : quartier, préférences d'alertes, choix de la langue

## Architecture

```
AquaAlerte/
├── App.xaml, App.xaml.cs        démarrage de l'application
├── AppShell.xaml / .cs          navigation (Shell) : onglets et routes
├── MauiProgram.cs               configuration de l'application
├── Models/                      données et logique d'affichage
│   ├── Signalement.cs           une coupure et ses textes d'affichage (+ Activite)
│   ├── BrouillonSignalement.cs  signalement en cours de saisie
│   ├── AlerteItem.cs            une alerte (+ AlerteGroupe)
│   └── DemoData.cs              données fictives et état en mémoire
├── Helpers/                     utilitaires
│   ├── TimeHelper.cs            heures et durées au format français
│   └── FieldHelper.cs           couleurs et état des composants
├── Views/                       un fichier XAML par écran, avec son code C#
├── Platforms/                   code propre à chaque plateforme
└── Resources/
    ├── Styles/                  couleurs et styles partagés (AquaStyles.xaml)
    ├── Images/                  images et icônes (SVG)
    └── Fonts/                   polices
```

### Les écrans (`Views/`)

| Écran | Rôle |
|---|---|
| `OnboardingPage` | Présentation en 3 écrans, avec « Passer » |
| `BienvenuePage` | Écran de bienvenue |
| `ConnexionPage`, `InscriptionPage` | Accès au compte |
| `QuartierPage` | Choix de la ville et du quartier |
| `AccueilPage` | Tableau de bord du quartier |
| `CartePage` | Carte schématique et fiche de la coupure |
| `DetailCoupurePage` | Détail, confirmations et activité |
| `SignalerPage` | Formulaire de signalement (étape 1 sur 2) |
| `RecapSignalementPage` | Récapitulatif avant publication (étape 2 sur 2) |
| `SignalementEnvoyePage` | Confirmation d'envoi et suivi de la coupure |
| `ObservationRecuePage` | Confirmation « la coupure continue » |
| `RetourEauPage` | Signalement du retour de l'eau |
| `AlertesPage` | Liste des alertes |
| `ProfilPage` | Préférences de l'utilisateur |
| `BottomNavBar` | Barre d'onglets du bas (composant réutilisé) |

## Navigation

La navigation repose sur **Shell** :

- **Onglets** (déclarés dans `AppShell.xaml`) : onboarding, bienvenue, accueil, carte, alertes et profil. On y accède par une route absolue, par exemple `//AccueilPage`.
- **Écrans empilés** (routes enregistrées dans `AppShell.xaml.cs`) : connexion, inscription, choix du quartier, signalement, récapitulatif, envoi, détail, observation reçue et retour de l'eau. Ils s'ouvrent par-dessus l'écran courant, et la flèche de retour ramène à l'écran précédent.

Parcours principaux :

- **Signaler** : Signaler → Récapitulatif → Signalement envoyé → Détail ou Accueil
- **Confirmer** : Détail → « Je constate toujours la coupure » → Observation reçue
- **Retour de l'eau** : Détail → « L'eau est revenue » → formulaire → Détail (statut « Eau revenue »)
- **Alertes** : toucher une alerte la marque comme lue et ouvre la coupure liée

## Données et stockage

- Les coupures, confirmations, alertes et signalements sont des **données fictives gardées en mémoire** (`DemoData`). Elles sont réinitialisées à la fermeture de l'application.
- Le quartier, la ville, les préférences d'alertes et la langue sont enregistrés sur le téléphone avec l'API `Preferences` de MAUI.

## Technologies

- .NET 10 et .NET MAUI
- XAML (interfaces, liaison de données avec `BindableLayout`) et C#
- Navigation Shell
- Images et icônes au format SVG
- Cible : Android

## Prérequis

- SDK .NET 10
- Workload MAUI pour Android : `dotnet workload install maui-android`
- SDK Android et JDK (installables avec l'extension « .NET MAUI » de VS Code)
- Un téléphone Android avec le débogage USB activé, ou un émulateur
- Une connexion Internet pour le premier build (téléchargement des paquets)

## Lancer le projet

```
git clone https://github.com/leal-login/AquaAlerte.git
cd AquaAlerte
dotnet build -t:Run -f net10.0-android
```

Le premier build peut durer plusieurs minutes.

## Limites connues

- Pas de serveur ni de base de données : les données ne sont pas partagées entre utilisateurs et les écrans de connexion et de création de compte n'authentifient personne.
- Les alertes sont écrites en dur ; les nouvelles actions de l'utilisateur n'en créent pas.
- Les interrupteurs d'alertes enregistrent le choix mais ne filtrent pas encore la liste.
- Le choix « English » est mémorisé, mais l'application reste en français (pas de traduction).
- Pas de notifications réelles, et le bouton « Utiliser ma position » n'est pas encore branché.
- La carte est un schéma : les positions sont approximatives.

## Phases du projet

| Phase | Contenu |
|---|---|
| A | Fondations : projet MAUI, architecture, couleurs et styles |
| B | Interfaces : onboarding, connexion, inscription, quartier, accueil, carte, signalement, alertes, profil |
| C | Navigation entre les écrans et validation sur téléphone |
| Suite | Fonctionnalités ajoutées une par une |

## Auteur

Projet réalisé par **Leal Tido** dans le cadre de l'examen de *Developpement mobile*
