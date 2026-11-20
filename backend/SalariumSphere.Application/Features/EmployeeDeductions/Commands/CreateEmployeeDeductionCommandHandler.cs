using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Common;
using SalariumSphere.Application.Features.EmployeeDeductions.Common;
using SalariumSphere.Application.Features.EmployeeDeductions.Queries;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.EmployeeDeductions.Commands;

public class CreateEmployeeDeductionCommandHandler : IRequestHandler<CreateEmployeeDeductionCommand, EmployeeDeductionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public CreateEmployeeDeductionCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<EmployeeDeductionDto> Handle(CreateEmployeeDeductionCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();

        // Verify employee exists and belongs to tenant
        var employee = await _context.Set<Employee>()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && e.TenantId == tenant.Id, cancellationToken);
        if (employee == null)
            throw new Exception($"Employee with ID {request.EmployeeId} not found.");

        var deduction = new EmployeeDeduction
        {
            Id = Guid.NewGuid(),
            EmployeeId = request.EmployeeId,
            DeductionName = request.DeductionName,
            Description = request.Description,
            CalculationType = request.CalculationType,
            FixedAmount = request.FixedAmount,
            FixedAmountCurrency = request.FixedAmountCurrency,
            PercentageValue = request.PercentageValue,
            TieredConfigJson = request.TieredConfigJson,
            CapAmount = request.CapAmount,
            CapCurrency = request.CapCurrency,
            ThresholdAmount = request.ThresholdAmount,
            ThresholdCurrency = request.ThresholdCurrency,
            IsActive = request.IsActive,
            TenantId = tenant.Id
        };

        await _context.Set<EmployeeDeduction>().AddAsync(deduction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return GetEmployeeDeductionsQueryHandler.MapToDto(deduction);
    }
}