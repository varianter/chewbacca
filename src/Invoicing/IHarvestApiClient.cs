using Invoicing.Models;

using Refit;

namespace Invoicing;

[Headers("User-Agent: Variant Harvest exporter", "Accept: application/json")]
public interface IHarvestApiClient
{
    const string HarvestAccountIdHeaderName = "Harvest-Account-ID";
    [Get("/projects")]
    Task<ProjectList> GetProjectList([Header(HarvestAccountIdHeaderName)] string accountId,
        [Authorize] string accessToken);

    // from and to must be date in yyyy-MM-dd format
    [Get("/time_entries?from={from}&to={to}&project_id={projectId}&page={page}")]
    Task<EntryModel> GetTimeEntries([Header(HarvestAccountIdHeaderName)] string accountId,
        [Authorize] string accessToken, string from, string to, int projectId, int page);
}