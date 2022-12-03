using Invoicing.Models;

using Microsoft.Extensions.Options;

using Shared;

namespace Invoicing
{
    public partial class HarvestService
    {
        private readonly IOptionsSnapshot<AppSettings> _appSettings;
        private readonly IHarvestApiClient _harvestApiClient;


        public HarvestService(IOptionsSnapshot<AppSettings> appSettings, IHarvestApiClient harvestApiClient)
        {
            _appSettings = appSettings;
            _harvestApiClient = harvestApiClient;
        }

        public HarvestConfig GetHarvestConfig(Office office)
        {
            return office switch
            {
                Office.Oslo => _appSettings.Value.Invoicing.Oslo,
                Office.Trondheim => _appSettings.Value.Invoicing.Trondheim,
                Office.Bergen => _appSettings.Value.Invoicing.Bergen,
                _ => throw new ArgumentOutOfRangeException(nameof(office), office,
                    "Only Oslo, Trondheim or Bergen is supported.")
            };
        }

        public async Task<List<MappedProject>> GetAllMappedEntries(HarvestConfig config, DateTime fromDate,
            DateTime toDate)
        {
            var projects = await _harvestApiClient.GetProjectList(config.AccountId, config.AccessToken);
            var tasks = projects.projects.Select(project =>
                MapEntriesToProject(config, project, fromDate, toDate));
            var mappedProjects = await Task.WhenAll(tasks);
            return mappedProjects.ToList();
        }

        private async Task<EntryModel> GetEntries(HarvestConfig config, int projectId, DateTime fromDate,
            DateTime toDate)
        {
            var to = toDate.ToString("yyyy-MM-dd");
            var from = fromDate.ToString("yyyy-MM-dd");

            var entryModel =
                await _harvestApiClient.GetTimeEntries(config.AccountId, config.AccessToken, from, to, projectId, 0);

            if (entryModel.total_pages <= 1)
            {
                return entryModel;
            }

            var allTimeEntries = new List<TimeEntry>();

            for (var i = 0; i < entryModel.total_pages; i++)
            {
                var nextEntries =
                    await _harvestApiClient.GetTimeEntries(config.AccountId, config.AccessToken, from, to, projectId,
                        i + 1);
                allTimeEntries.AddRange(nextEntries!.time_entries);
            }

            entryModel.time_entries = allTimeEntries;

            return entryModel;
        }

        private async Task<MappedProject> MapEntriesToProject(HarvestConfig config, Project project, DateTime fromDate,
            DateTime toDate)
        {
            var entryModel = await GetEntries(config, project.id, fromDate, toDate);
            var mappedEntries = new MappedProject
            {
                project = project,
                month = GetMonth(fromDate.Month),
                year = fromDate.Year,
                numberOfDays = toDate.Day,
                initialDate = fromDate
            };

            var monthlyEntries = new List<UserHours>();

            foreach (var entry in entryModel.time_entries)
            {
                var foundUserEntry = monthlyEntries.Find(
                    userHours => userHours.user.id == entry.user.id
                );

                var newHours = new Hours() { spent_date = entry.spent_date, spent_hours = entry.hours };

                if (foundUserEntry != null)
                {
                    foundUserEntry.spent_hours_by_date.Add(newHours);
                    foundUserEntry.sum += entry.hours;
                }
                else
                {
                    var newUserEntry = new UserHours()
                    {
                        user = entry.user, spent_hours_by_date = new List<Hours> { newHours }
                    };
                    newUserEntry.sum += entry.hours;
                    monthlyEntries.Add(newUserEntry);
                }
            }

            monthlyEntries = ParseHours(monthlyEntries, toDate.Day);

            mappedEntries.monthly_entries = monthlyEntries;

            return mappedEntries;
        }

        private static List<UserHours> ParseHours(List<UserHours> monthlyHours, int numberOfDays)
        {
            foreach (var userEntries in monthlyHours)
            {
                userEntries.parsed_hours = new List<double>();
                var parsedHours = new List<double>();
                for (var i = 0; i < numberOfDays; i++)
                {
                    var temp = userEntries.spent_hours_by_date.Find(hours => hours.spent_date.Day == i + 1);
                    if (temp != null)
                    {
                        parsedHours.Add(temp.spent_hours);
                    }
                    else
                    {
                        parsedHours.Add(0);
                    }
                }

                userEntries.parsed_hours = parsedHours;
            }

            return monthlyHours;
        }


        private static string GetMonth(int number)
        {
            string[] month =
            {
                "Januar", "Februar", "Mars", "April", "Mai", "Juni", "Juli", "August", "September", "Oktober",
                "November", "Desember"
            };
            return month[number - 1];
        }
    }
}