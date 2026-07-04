using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Application.Settings;
using TrojesDeMaranon.Persistence.Context;

namespace TrojesDeMaranon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/company-settings")]
public sealed class CompanySettingsController(AppDbContext db, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<CompanySettingsDto>>> Get(CancellationToken cancellationToken)
    {
        var companyId = currentUser.CompanyId;
        if (companyId is null)
        {
            return Unauthorized(ApiResponse<CompanySettingsDto>.Fail("Sesion sin empresa."));
        }
        var settings = await db.CompanySettings.AsNoTracking().FirstOrDefaultAsync(x => x.CompanyId == companyId, cancellationToken);
        return settings is null ? NotFound(ApiResponse<CompanySettingsDto>.Fail("Configuracion no encontrada.")) : ApiResponse<CompanySettingsDto>.Ok(ToDto(settings));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<CompanySettingsDto>>> Update(UpdateCompanySettingsRequest request, CancellationToken cancellationToken)
    {
        var companyId = currentUser.CompanyId;
        if (companyId is null)
        {
            return Unauthorized(ApiResponse<CompanySettingsDto>.Fail("Sesion sin empresa."));
        }
        var settings = await db.CompanySettings.FirstOrDefaultAsync(x => x.CompanyId == companyId, cancellationToken);
        if (settings is null)
        {
            return NotFound(ApiResponse<CompanySettingsDto>.Fail("Configuracion no encontrada."));
        }
        settings.AllowNegativeInventory = request.AllowNegativeInventory;
        settings.MaterialDeviationAlertPercent = request.MaterialDeviationAlertPercent;
        settings.DieselAnomalyPercent = request.DieselAnomalyPercent;
        settings.DefaultCurrency = request.DefaultCurrency.ToUpperInvariant();
        settings.TimeZone = request.TimeZone;
        settings.RequireEvidenceOnReceipts = request.RequireEvidenceOnReceipts;
        settings.RequireEvidenceOnIssues = request.RequireEvidenceOnIssues;
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<CompanySettingsDto>.Ok(ToDto(settings));
    }

    private static CompanySettingsDto ToDto(Domain.Companies.CompanySettings x) => new(
        x.Id,
        x.CompanyId,
        x.AllowNegativeInventory,
        x.MaterialDeviationAlertPercent,
        x.DieselAnomalyPercent,
        x.DefaultCurrency,
        x.TimeZone,
        x.RequireEvidenceOnReceipts,
        x.RequireEvidenceOnIssues);
}
