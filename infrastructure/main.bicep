@description('The base name for resources')
param name string = 'chewie'

@description('The location for resources')
param location string = resourceGroup().location

@description('The web site hosting plan')
param sku string = 'S1'

@description('Specifies sql admin password')
@secure()
param sqlAdministratorPassword string = 'P${uniqueString(resourceGroup().id, '224F5A8B-51DB-46A3-A7C8-59B0DD584A41')}x!'

var hostingPlanName = '${name}-sp-${uniqueString(resourceGroup().id)}'
var applicationInsightsName = '${name}-insights-${uniqueString(resourceGroup().id)}'
var webAppName = '${name}-webapp-${uniqueString(resourceGroup().id)}'
var appConfName = '${name}-cfg-${uniqueString(resourceGroup().id)}'
var sqlServerName = '${name}-sql-${uniqueString(resourceGroup().id)}'
var sqlDatabaseName = '${name}-db-${uniqueString(resourceGroup().id)}'
var keyVaultName = '${name}-kv-${uniqueString(resourceGroup().id)}'

resource plan 'Microsoft.Web/serverfarms@2020-12-01' = {
  name: hostingPlanName
  location: location
  sku: {
    name: sku
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

module config 'modules/config.bicep' = {
  name: appConfName
  params: {
    appConfName: appConfName
    location: location
    keyVaultName: keyVaultName
    webPrincipalId: web.identity.principalId
  }
}

resource web 'Microsoft.Web/sites@2020-12-01' = {
  name: webAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {

    httpsOnly: true
    serverFarmId: plan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|7.0'
      minTlsVersion: '1.2'
      httpLoggingEnabled: true
      logsDirectorySizeLimit: 35
    }
  }
}

module sql 'modules/sql.bicep' = {
  name: sqlServerName
  params: {
    sqlServerName: sqlServerName
    sqlDatabaseName: sqlDatabaseName
    location: location
    sqlAdministratorLogin: 'l${uniqueString(resourceGroup().id, '9A08DDB9-95A1-495F-9263-D89738ED4205')}'
    sqlAdministratorPassword: sqlAdministratorPassword
    adminIdentitySid: web.identity.principalId
    tenant: web.identity.tenantId
  }
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
  }
}

// Add connectionStrings and config to webapp
resource webAppSettings 'Microsoft.Web/sites/config@2022-03-01' = {
  name: 'web'
  kind: 'string'
  parent: web
  properties: {
    appSettings: [
      {
        name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
        value: applicationInsights.properties.InstrumentationKey
      }
      {
        name: 'AppSettings__UseAzureAppConfig'
        value: 'true'
      }
      {
        name: 'AppSettings__AzureAppConfigUri'
        value: config.outputs.appConfigEndpoint
      }
    ]
    connectionStrings: [
      {
        name: 'EmployeeDatabase'
        connectionString: sql.outputs.sqlConnectionString
      }
    ]
  }
}

output siteUrl string = 'https://${web.properties.defaultHostName}/'
