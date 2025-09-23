using System.Globalization;
using System.Net;

using ApplicationCore.Interfaces;

using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

using Shared;

namespace Infrastructure.Repositories;

public class BlobStorageRepository : IBlobStorageRepository
{
    private readonly IFeatureManager _featureManager;
    private readonly IOptionsSnapshot<AppSettings> _appSettings;
    private ILogger<BlobStorageRepository> _logger;

    private const string BlobMetadataLastModifiedKey = "lastModified";

    /**
     * <summary> Class for handling blob storage </summary>
     * <param name="appSettings"> Options for connection string and which container to add to </param>
     * <param name="logger">ILogger interface for logging</param>
     */
    public BlobStorageRepository(IFeatureManager featureManager, IOptionsSnapshot<AppSettings> appSettings, ILogger<BlobStorageRepository> logger)
    {
        _featureManager = featureManager;
        _appSettings = appSettings;
        _logger = logger;
    }

    /**
     * <summary>Copies the image at employeeImageUri and streams it into a Blob Block in Azure</summary>
     * <param name="cvPartnerUserId">Name of the employee. Will be the name of the file in the blob storage</param>
     * <param name="employeeImageUri">URL to the employee image. Used to copy it to the blob storage</param>
     */
    public async Task<string?> SaveToBlob(string cvPartnerUserId, string employeeImageUri)
    {
        Uri uri = new(employeeImageUri);

        var container = await GetBlobContainerClient();

        var blockBlobClient = container.GetBlobClient($"{cvPartnerUserId}.png");

        using var client = new HttpClient();
        var message = new HttpRequestMessage(HttpMethod.Get, uri);

        if (await blockBlobClient.ExistsAsync())
        {
            var props = await blockBlobClient.GetPropertiesAsync();
            message.Headers.IfModifiedSince =
                DateTimeOffset.TryParse(props.Value?.Metadata?[BlobMetadataLastModifiedKey], out DateTimeOffset blobLastModifiedDate)
                    ? blobLastModifiedDate
                    : null;
        }

        var response = await client.SendAsync(message);
        if (response.StatusCode == HttpStatusCode.NotModified)
        {
            if (await _featureManager.IsEnabledAsync("ForceImageUploadToBlobStorage"))
            {
                _logger.LogWarning(
                    "No need to update the blob for {CvPartnerUserId} as the image has not changed, but the feature flag is enabled to force the upload anyway",
                    cvPartnerUserId);
            }
            else
            {
                // No need to update the blob
                _logger.LogInformation(
                    "No need to update the blob for {CvPartnerUserId} as the image has not changed",
                    cvPartnerUserId);
                return blockBlobClient.Uri.AbsoluteUri;
            }
        }

        if (!response.IsSuccessStatusCode)
        {
            // No need to update the blob
            _logger.LogError(
                "Error fetching image {CvPartnerUserId}. Status Code: {StatusCode}",
                cvPartnerUserId, response.StatusCode);
            return null;
        }

        var lastModifiedDate = response.Content.Headers.LastModified ?? DateTimeOffset.UtcNow;
        var lastModified = lastModifiedDate.ToString(CultureInfo.InvariantCulture);
        var stream = await blockBlobClient.OpenWriteAsync(true);
        await response.Content.CopyToAsync(stream);
        await stream.FlushAsync();
        await stream.DisposeAsync();

        await blockBlobClient.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = "image/png" });
        await blockBlobClient.SetMetadataAsync(new Dictionary<string, string>
        {
            { "name", cvPartnerUserId }, { BlobMetadataLastModifiedKey, lastModified }
        });

    return blockBlobClient.Uri.AbsoluteUri;
    }

    private async Task<BlobContainerClient> GetBlobContainerClient()
    {
        BlobContainerClient container = _appSettings.Value.BlobStorage.UseDevelopmentStorage
            ? new BlobContainerClient("UseDevelopmentStorage=true", "employees")
            : new BlobContainerClient(_appSettings.Value.BlobStorage.Endpoint, new DefaultAzureCredential());

        await container.CreateIfNotExistsAsync();
        return container;
    }

    public async Task DeleteBlob(string blobUrlToBeDeleted)
    {
        var container = await GetBlobContainerClient();
        var blobName = blobUrlToBeDeleted.Split('/').Last();
        var blobClient = container.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }
}