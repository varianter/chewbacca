<div align="center">
    <img src="docs/logo.png" width="200" height="200">
    <h1>Chewbacca</h1>

</div>

> "La Chewbacca ta seg av de tekniske detaljene, mens du tar deg av datagalaksens utfordringer og triumfer!"

Variant har mange internesystemer (UniEconomy, Harvest, CVPartner etc.) og denne løsningen fungerer som en proxy og cache for løsninger som ønsker å bruke data fra disse systemene.

Løsningen er bygget slik at arbeid på den gir relevant erfaring for hva vi møter ute hos våre kunder. [Les mer om det her](docs/architecture.md).

Det er tatt noen avgjørelser rundt arkitektur og hvordan vi bruker skytjenester. De avgjørelsene kan du [lese mer om her](docs/architecture.md).

## Kommandoer

For å kjøre
```dotnet run --project src/Web```

For å kjøre migrering til Employees-databasen
```dotnet ef migrations add <navn på migrasjon> --project src/Infrastructure --startup-project src/Web``` 

## Up and running med ekte integrasjoner

For å få tilgang til integrasjoner må man ha:

1. En variant-bruker/epost
2. Blitt lagt til i _developers_ gruppen i Microsoft Entra ID (tidligere kalt Azure AD)
3. Installert _Azure CLI_ og kjørt `az login`

Man kan da få konfigurasjon og secrets fra azure uten noe ekstra oppsett gjennom Azure App Configuration. Når man utvikler kjører man opp SQL Server og Blob Storage lokalt, siden det er disse to tjenestene som skrives til - så de er grei å ha kontroll på selv. De er definert i `docker-compose.yml`. Installer Docker Desktop og kjør `docker-compose up -d`.

### Manuell synkronisering

Data fra integrasjoner hentes inn og synkroniseres med databasen én gang i døgnet. Under utvikling er det nyttig å kjøre datainnhenting ved behov. Dette kan gjøres ved å kalle `/Orchestrator`-endepunktet (f.eks. via Swagger UI). Her kreves autorisering, som beskrevet i [Auth](#Auth).

## Auth

For å gjøre kall til endepunkter som er bak _auth_ kan man kjøre `az account get-access-token --scope api://chewbacca/employees` for å få et gyldig token. Man kan da trykke Authorize knappen øverst til høyre i Swagger UI på `/swagger/index.html` og lime inn tokenet der: `Bearer <token>`. Man kan da kalle de låste endepunktene.

Om man lager en App Registration i vår Azure Tenant hvor access token skal kunne brukes til å hente data fra Chewbacca må man huske å legge til scopet `api://chewbacca/employees` under API Permissions og få det godkjent av en Administrator.

## Infrastructure

Work in progress. Se i `infrastructure`-mappa

```bash
az deployment group create --resource-group my-test-group --template-file .\infrastructure\azuredeploy.bicep --location westeurope`
```

## Feilsøking

### Bygg feiler på MacOS med Norsk systemspråk

Dersom du utvikler på en maskin med MacOS med Norsk systemspråk vil du sannsynligvis møte en
byggfeil som sier at versjonen av MSBuild ikke er gyldig for å bygge Refit-avhengigheten vi bruker. Denne feilen oppstår fordi MSBuild-versjonen kan ha ulike formater for ulike språk, noe som fører til at
Refit sin versjonssammenligning feiler.

**Løsning**

Som forklart i [denne artikkelen](https://learn.microsoft.com/en-us/dotnet/core/runtime-config/globalization) 
kan man be MSBuild bygge uten å bruke språkavhengig formatering for tekster, datoer ol.:

    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 dotnet build


Man kan også sette `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1` som en miljøvariabel for utviklermaskinen, og dermed slipp å oppgi variablen i selve byggkommandoen.

### "The following tenants don't contain accessible subscriptions"

Når du logger inn med az login, sjekkes det om du er med i _developers_ gruppen i Microsoft Entra ID (tidligere kalt Azure AD). Dersom du har forsøkt å logge inn _før_ du er medlem i gruppa, kan cachen være feil.

**Løsning**

Dersom du ikke er med å gruppa, må du be noen om å legge deg til. Hvis problemet er med cache, kan du prøve å kjøre `az account clear` og så logge på igjen. Mer informasjon finnes eventuelt [her](https://learn.microsoft.com/en-us/cli/azure/manage-azure-subscriptions-azure-cli?tabs=bash#clear-your-subscription-cache).


### "Could not open a connection to SQL Server" eller AzureSQL edge-container i Docker feiler

Ved oppsett av prosjektet oppstår det feil med å koble til SQL serveren. Dette kan gi feil ved tilkobling eller hvis man sjekker Docker.

**Løsning**

Nyeste versjon (4.38.0)  av Docker Desktop fungerte ikke i skrivende stund. [Versjon 4.32.0](https://docs.docker.com/desktop/release-notes/#4320) fungerte derimot fint.