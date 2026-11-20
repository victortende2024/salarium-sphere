using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Common;
using SalariumSphere.Application.Features.EmployeeDeductions.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.EmployeeDeductions.Queries;

public class GetEmployeeDeductionsQueryHandler : IRequestHandler<GetEmployeeDeductionsQuery, List<EmployeeDeductionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public GetEmployeeDeductionsQueryHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<List<EmployeeDeductionDto>> Handle(GetEmployeeDeductionsQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        var deductions = await _context.Set<EmployeeDeduction>()
            .Where(d => d.EmployeeId == request.EmployeeId && d.TenantId == tenant.Id)
            .OrderBy(d => d.DeductionName)
            .ToListAsync(cancellationToken);
        return deductions.Select(MapToDto).ToList();
    }

    public static EmployeeDeductionDto MapToDto(EmployeeDeduction ded) => new()
    {
        Id = ded.Id,
        EmployeeId = ded.EmployeeId,
        DeductionName = ded.DeductionName,
        Description = ded.Description,
        CalculationType = ded.CalculationType,
        FixedAmount = ded.FixedAmount,
        FixedAmountCurrency = ded.FixedAmountCurrency,
        PercentageValue = ded.PercentageValue,
        TieredConfigJson = ded.TieredConfigJson,
        CapAmount = ded.CapAmount,
        CapCurrency = ded.CapCurrency,
        ThresholdAmount = ded.ThresholdAmount,
        ThresholdCurrency = ded.ThresholdCurrency,
        IsActive = ded.IsActive
    };
}
