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
2. Blitt lagt til i _developers_ gruppen i Azure AD
3. Installert _Azure CLI_ og kjørt `az login`

Man kan da få konfigurasjon og secrets fra azure uten noe ekstra oppsett gjennom Azure App Configuration. Når man utvikler kjører man opp SQL Server og Blob Storage lokalt, siden det er disse to tjenestene som skrives til - så de er grei å ha kontroll på selv. De er definert i `docker-compose.yml`. Installer Docker Desktop og kjør `docker-compose up -d`.

## Auth

For å gjøre kall til endepunkter som er bak _auth_ kan man kjøre `az account get-access-token --scope api://chewbacca/employees` for å få et gyldig token. Man kan da trykke Authorize knappen øverst til høyre i Swagger UI på `/swagger/index.html` og lime inn tokenet der: `Bearer <token>`. Man kan da kalle de låste endepunktene.

For å populere den lokale databasen må du kjøre `/Orchestrator`-endepunktet i Swagger UI.

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
