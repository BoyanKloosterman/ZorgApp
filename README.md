# ZorgApp

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Unity](https://img.shields.io/badge/built%20with-Unity-000?logo=unity)](https://unity.com/)
[![ASP.NET Core](https://img.shields.io/badge/backend-ASP.NET_Core-blue?logo=dotnet)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/platform-Cross--platform-success)]()
[![Status](https://img.shields.io/badge/status-In%20Development-yellow)]()

ZorgApp is een applicatie ontwikkeld voor kinderen van 7 tot 12 jaar met diabetes type 1. De app helpt jonge patiënten hun behandelingstraject te begrijpen via een speelse en visuele benadering. Met ZorgApp worden dagelijkse handelingen ondersteund, krijgen kinderen meldingen voor belangrijke acties en kunnen ze op een eenvoudige manier hun ervaringen en vragen vastleggen.

---

## 📱 Belangrijkste Features

- **🔔 Notificatiesysteem**  
  ZorgApp herinnert kinderen (en hun ouders/verzorgers) op tijd aan belangrijke momenten, zoals medicatie-inname, glucosemetingen en afspraken.

- **📝 Notitiesysteem**  
  Kinderen kunnen noteren hoe ze zich voelen, vragen opschrijven voor de arts of waarden registreren zoals hun bloedsuikerspiegel.

- **🛤️ Trajectsysteem**  
  Een visuele weergave van het behandeltraject toont welke stappen er zijn (zoals ziekenhuisbezoeken en cursussen), in begrijpelijke taal voor kinderen.

---

## 🛠️ Installatie-instructies

ZorgApp bestaat uit twee delen: een Unity-frontend en een ASP.NET Core-backend.

### 1. Repository klonen

```bash
git clone https://github.com/BoyanKloosterman/ZorgApp.git
```

### 2. Project openen

- **Frontend**: Open de `ZorgAppFrontend` map met de Unity Editor.
- **Backend**: Open de `ZorgAppAPI` map met Visual Studio (of een andere IDE met .NET ondersteuning).
- **Extra**: Afhankelijk van de Unity-versie en ASP.NET Core-versie kan aanvullende setup nodig zijn (bijvoorbeeld het installeren van de juiste Unity versie of .NET SDK). Raadpleeg de documentatie van Unity en ASP.NET indien nodig. Let op: deze repository bevat geen expliciete instructies voor Unity- of .NET-versies, dus zorg dat je een recente LTS-versie van beide gebruikt voor beste compatibiliteit.
### 3. Vereisten

- Unity (recente LTS-versie aanbevolen)
- .NET SDK (recente LTS-versie aanbevolen)

Raadpleeg de officiële documentatie van [Unity](https://unity.com) en [.NET](https://dotnet.microsoft.com/) voor installatie-instructies.

---

## 💻 Gebruikte technologieën

- **Frontend**: Unity (C#) – voor een interactieve, kindvriendelijke gebruikerservaring.
- **Backend**: ASP.NET Core (C#) – voor een veilige en schaalbare web API.

---

## 📂 Projectstructuur

```
ZorgApp/
├── ZorgAppFrontend/       # Unity-project: UI, gameplay, notificaties
├── ZorgAppAPI/            # ASP.NET Core Web API: controllers, modellen, database
└── ZorgAppAPI.Tests/      # Unit tests voor backend logica
```

---

## 👥 Contributors & Commits

Onderstaand een overzicht van het aantal commits per contributor in deze repository. Dit laat zien hoeveel bijdragen (codewijzigingen) ieder teamlid heeft geleverd:

| Contributor       | Aantal Commits |
|-------------------|----------------|
| Boyan Kloosterman | 112            |
| Pluk Zwaal        | 27             |
| Jeroen Zwaal      | 17             |
| Junbo Chen        | 4              |

> Bovenstaande telling is gebaseerd op de commit-historie van de main branch. Het geeft een indicatie van ieders bijdrage. Let op dat kwaliteit en rolverdeling niet enkel uit commit-aantallen blijken – bijvoorbeeld een kleinere hoeveelheid commits kan nog steeds cruciale functionaliteit bevatten.

---

## 🤝 Bijdragen

Wil je bijdragen aan ZorgApp? Fork deze repository, maak een feature branch, en dien een pull request in. Issues en feedback zijn welkom!
