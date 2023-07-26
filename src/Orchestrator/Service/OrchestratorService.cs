using Bemanning.Repositories;
using SoftRig.Service;
using BlobStorage.Service;

using CvPartner.Service;

using Employees.Models;
using Employees.Service;

using Microsoft.Extensions.Logging;

namespace Orchestrator.Service;

public class OrchestratorService
{
    private readonly EmployeesService _employeesService;
    private readonly CvPartnerService _cvPartnerService;
    private readonly IBemanningRepository _bemanningRepository;
    private readonly ISoftRigService _softRigService;
    private readonly BlobStorageService _blobStorageService;
    private readonly ILogger<OrchestratorService> _logger;
    private readonly FilteredUids _filteredUids;

    public OrchestratorService(EmployeesService employeesService, CvPartnerService cvPartnerService,
        IBemanningRepository bemanningRepository,
        BlobStorageService blobStorageService,
        FilteredUids filteredUids,
        ILogger<OrchestratorService> logger,
        ISoftRigService softRigService)
    {
        _employeesService = employeesService;
        _cvPartnerService = cvPartnerService;
        _bemanningRepository = bemanningRepository;
        _softRigService = softRigService;
        _blobStorageService = blobStorageService;
        _logger = logger;
        _filteredUids = filteredUids;
    }

    public async Task FetchMapAndSaveEmployeeData()
    {
        _logger.LogInformation("OrchestratorRepository: FetchMapAndSaveEmployeeData: Started");
        var bemanningEntries = await _bemanningRepository.GetBemanningDataForEmployees();
        var cvEntries = await _cvPartnerService.GetCvPartnerEmployees();
        var softRigEmployees = await _softRigService.GetSoftRigEmployees();

        var phoneNumberUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();

        foreach (var bemanning in bemanningEntries.Where(IsActiveEmployee))
        {
            var cv = cvEntries.Find(cv => cv.email.ToLower().Trim() == bemanning.Email.ToLower().Trim());
            var matchingSoftRigEmployee = softRigEmployees.Find(employee => employee.BusinessRelationInfo!.DefaultEmail!.EmailAddress.ToLower().Trim() == bemanning.Email.ToLower().Trim());

            if (cv != null)
            {
                var countryCode = cv.email.ToLower().EndsWith(".se") ? "SE" : "NO";

                var phoneNumber = phoneNumberUtil.IsPossibleNumber(cv.telephone, countryCode)
                    ? phoneNumberUtil.Format(phoneNumberUtil.Parse(cv.telephone, countryCode),
                        PhoneNumbers.PhoneNumberFormat.E164)
                    : null;

                var isFilteredPhone = _filteredUids.Uids.Contains(cv.user_id);

                var employee = new EmployeeEntity
                {
                    Name = cv.name,
                    Email = cv.email,
                    Telephone = isFilteredPhone ? null : phoneNumber,
                    ImageUrl =
                        cv.image.url != null
                            ? await _blobStorageService.SaveToBlob(cv.user_id, cv.image.url)
                            : null,
                    OfficeName = cv.office_name,
                    StartDate = bemanning.StartDate,
                    EndDate = bemanning.EndDate,
                    CountryCode = countryCode.ToLower()
                };

                if (matchingSoftRigEmployee != null)
                {
                    var softRigEmployeeDto = SoftRig.Models.ModelConverters.ToEmployeeDto(matchingSoftRigEmployee);
                    employee.AccountNumber = softRigEmployeeDto!.AccountNumber;
                    employee.Address = softRigEmployeeDto!.Address;
                    employee.ZipCode = softRigEmployeeDto!.ZipCode;
                    employee.City = softRigEmployeeDto!.City;
                }


                await _employeesService.AddOrUpdateEmployee(employee);
            }
            else
            {
                // If the employee does not exist in CV Partner, only in Bemanning, we should ensure the employee is not in the database.
                _logger.LogInformation(
                    "Deleting employee with email {BemanningEmail} from database, since it does not exist in CV Partner",
                    bemanning.Email);
                var blobUrlToBeDeleted = await _employeesService.EnsureEmployeeIsDeleted(bemanning.Email);
                if (blobUrlToBeDeleted == null)
                {
                    continue;
                }

                _logger.LogInformation("Deleting blob with url {BlobUrlToBeDeleted}", blobUrlToBeDeleted);
                await _blobStorageService.DeleteBlob(blobUrlToBeDeleted);
            }
        }

        var blobUrlsToBeDeleted = await _employeesService.EnsureEmployeesWithEndDateBeforeTodayAreDeleted();
        foreach (var blobUrlToBeDeleted in blobUrlsToBeDeleted)
        {
            _logger.LogInformation("Deleting blob with url {BlobUrlToBeDeleted}", blobUrlToBeDeleted);
            if (blobUrlToBeDeleted != null)
            {
                await _blobStorageService.DeleteBlob(blobUrlToBeDeleted);
            }
        }

        _logger.LogInformation("OrchestratorRepository: FetchMapAndSaveEmployeeData: Finished");
    }

    private static bool IsActiveEmployee(BemanningEmployee bemanning)
    {
        return DateTime.Now >= bemanning.StartDate && (bemanning.EndDate == null || DateTime.Now <= bemanning.EndDate);
    }
}