using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Common;
using SalariumSphere.Application.Features.EmployeeDeductions.Common;
using SalariumSphere.Application.Features.EmployeeDeductions.Queries;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.EmployeeDeductions.Commands;

public class UpdateEmployeeDeductionCommandHandler : IRequestHandler<UpdateEmployeeDeductionCommand, EmployeeDeductionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public UpdateEmployeeDeductionCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<EmployeeDeductionDto> Handle(UpdateEmployeeDeductionCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        var ded = await _context.Set<EmployeeDeduction>()
            .FirstOrDefaultAsync(d => d.Id == request.Id && d.TenantId == tenant.Id, cancellationToken);
        if (ded == null)
            throw new Exception($"Deduction with ID {request.Id} not found.");

        ded.DeductionName = request.DeductionName;
        ded.Description = request.Description;
        ded.CalculationType = request.CalculationType;
        ded.FixedAmount = request.FixedAmount;
        ded.FixedAmountCurrency = request.FixedAmountCurrency;
        ded.PercentageValue = request.PercentageValue;
        ded.TieredConfigJson = request.TieredConfigJson;
        ded.CapAmount = request.CapAmount;
        ded.CapCurrency = request.CapCurrency;
        ded.ThresholdAmount = request.ThresholdAmount;
        ded.ThresholdCurrency = request.ThresholdCurrency;
        ded.IsActive = request.IsActive;
        ded.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return GetEmployeeDeductionsQueryHandler.MapToDto(ded);
    }
}