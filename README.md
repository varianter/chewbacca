<div align="center">
    <img src="docs/logo.png" width="200" height="200">
    <h1>Chewbacca</h1>

</div>

> "La Chewbacca ta seg av de tekniske detaljene, mens du tar deg av datagalaksens utfordringer og triumfer!"

Variant har mange internesystemer (UniEconomy, Harvest, CVPartner etc.) og denne løsningen fungerer som en proxy og cache for løsninger som ønsker å bruke data fra disse systemene.

Løsningen er bygget slik at arbeid på den gir relevant erfaring for hva vi møter ute hos våre kunder. [Les mer om det her](docs/relevance.md).

Det er tatt noen avgjørelser rundt arkitektur og hvordan vi bruker skytjenester. De avgjørelsene kan du [lese mer om her](docs/architecture.md).

## Up and running med ekte integrasjoner

For å få tilgang til integrasjoner må man ha:

1. En variant-bruker/epost
2. Blitt lagt til i _developers_ gruppen i Azure AD
3. Installert _Azure CLI_ og kjørt `az login`

Man kan da få konfigurasjon og secrets fra azure uten noe ekstra oppsett gjennom Azure App Configuration. Når man utvikler kjører man opp SQL Server og Blob Storage lokalt, siden det er disse to tjenestene som skrives til - så de er grei å ha kontroll på selv. De er definert i `docker-compose.yml`. Installer Docker Desktop og kjør `docker-compose up -d`.

## Infrastructure

Work in progress. Se i `infrastructure`-mappa

```bash
az deployment group create --resource-group my-test-group --template-file .\infrastructure\azuredeploy.bicep --location westeurope`
```

## Struktur
Skisse over hvordan applikasjonen er bygget opp finner man [her](./docs/Structure/Structure.md)

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
