using Invoicing;

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApi;

public class Fakturagrunnlag : PageModel
{
    private readonly HarvestService _harvestService;

    public Fakturagrunnlag(HarvestService harvestService)
    {
        _harvestService = harvestService;
    }
    public void OnGet()
    {
    }
}