using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Application.Companies;
using TrojesDeMaranon.Domain.Companies;
using TrojesDeMaranon.Persistence.Context;

namespace TrojesDeMaranon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/companies")]
public sealed class CompaniesController(AppDbContext db, IValidator<UpsertCompanyRequest> companyValidator, IValidator<CreateBranchRequest> branchValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<IReadOnlyList<CompanyDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await db.Companies.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CompanyDto(x.Id, x.Name, x.LegalName, x.TaxId, x.FiscalAddress, x.Phone, x.Email, x.IsActive))
            .ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<CompanyDto>>.Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CompanyDto>>> Get(Guid id, CancellationToken cancellationToken)
    {
        var company = await db.Companies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return company is null
            ? NotFound(ApiResponse<CompanyDto>.Fail("Empresa no encontrada."))
            : ApiResponse<CompanyDto>.Ok(ToDto(company));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CompanyDto>>> Create(UpsertCompanyRequest request, CancellationToken cancellationToken)
    {
        await companyValidator.ValidateAndThrowAsync(request, cancellationToken);
        var company = new Company
        {
            Name = request.Name,
            LegalName = request.LegalName,
            TaxId = request.TaxId,
            FiscalAddress = request.FiscalAddress,
            Phone = request.Phone,
            Email = request.Email,
            IsActive = true
        };
        db.Companies.Add(company);
        db.CompanySettings.Add(new CompanySettings { CompanyId = company.Id });
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = company.Id }, ApiResponse<CompanyDto>.Ok(ToDto(company)));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CompanyDto>>> Update(Guid id, UpsertCompanyRequest request, CancellationToken cancellationToken)
    {
        await companyValidator.ValidateAndThrowAsync(request, cancellationToken);
        var company = await db.Companies.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (company is null)
        {
            return NotFound(ApiResponse<CompanyDto>.Fail("Empresa no encontrada."));
        }
        company.Name = request.Name;
        company.LegalName = request.LegalName;
        company.TaxId = request.TaxId;
        company.FiscalAddress = request.FiscalAddress;
        company.Phone = request.Phone;
        company.Email = request.Email;
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<CompanyDto>.Ok(ToDto(company));
    }

    [HttpGet("{id:guid}/branches")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BranchDto>>>> GetBranches(Guid id, CancellationToken cancellationToken)
    {
        if (!await db.Companies.AnyAsync(x => x.Id == id, cancellationToken))
        {
            return NotFound(ApiResponse<IReadOnlyList<BranchDto>>.Fail("Empresa no encontrada."));
        }
        var branches = await db.Branches.AsNoTracking()
            .Where(x => x.CompanyId == id)
            .OrderBy(x => x.Name)
            .Select(x => new BranchDto(x.Id, x.CompanyId, x.Code, x.Name, x.Address, x.Phone, x.IsActive))
            .ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<BranchDto>>.Ok(branches);
    }

    [HttpPost("{id:guid}/branches")]
    public async Task<ActionResult<ApiResponse<BranchDto>>> CreateBranch(Guid id, CreateBranchRequest request, CancellationToken cancellationToken)
    {
        await branchValidator.ValidateAndThrowAsync(request, cancellationToken);
        if (!await db.Companies.AnyAsync(x => x.Id == id, cancellationToken))
        {
            return NotFound(ApiResponse<BranchDto>.Fail("Empresa no encontrada."));
        }
        var branch = new Branch
        {
            CompanyId = id,
            Code = request.Code,
            Name = request.Name,
            Address = request.Address,
            Phone = request.Phone,
            IsActive = true
        };
        db.Branches.Add(branch);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<BranchDto>.Ok(new BranchDto(branch.Id, branch.CompanyId, branch.Code, branch.Name, branch.Address, branch.Phone, branch.IsActive));
    }

    private static CompanyDto ToDto(Company company) => new(company.Id, company.Name, company.LegalName, company.TaxId, company.FiscalAddress, company.Phone, company.Email, company.IsActive);
}
