using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Common;
using SalariumSphere.Application.Features.EmployeeDeductions.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.EmployeeDeductions.Queries;

public class GetEmployeeDeductionByIdQueryHandler : IRequestHandler<GetEmployeeDeductionByIdQuery, EmployeeDeductionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public GetEmployeeDeductionByIdQueryHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<EmployeeDeductionDto> Handle(GetEmployeeDeductionByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        var ded = await _context.Set<EmployeeDeduction>()
            .FirstOrDefaultAsync(d => d.Id == request.Id && d.TenantId == tenant.Id, cancellationToken);
        if (ded == null)
            throw new Exception($"Deduction with ID {request.Id} not found.");
        return GetEmployeeDeductionsQueryHandler.MapToDto(ded);
    }
}