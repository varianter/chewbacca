﻿namespace Shared;

public record AppSettings
{
    public Uri AzureAppConfigUri { get; set; }
    public bool UseAzureAppConfig { get; set; }
}