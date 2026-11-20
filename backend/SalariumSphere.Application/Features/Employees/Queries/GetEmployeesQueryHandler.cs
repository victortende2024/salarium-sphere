using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.Employees.Common;
using SalariumSphere.Application.Interfaces;

namespace SalariumSphere.Application.Features.Employees.Queries;

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, List<EmployeeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEmployeesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmployeeDto>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Domain.Entities.Employee>()
            .Include(e => e.Company)
            .Include(e => e.Department)
            .Include(e => e.PrimaryProfile)
            .Include(e => e.EmployeeSalaryStructures)
            .Include(e => e.EmployeeCustomBenefits)
            .AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(e => e.IsActive);

        if (request.CompanyId.HasValue)
            query = query.Where(e => e.CompanyId == request.CompanyId.Value);

        if (request.DepartmentId.HasValue)
            query = query.Where(e => e.DepartmentId == request.DepartmentId.Value);

        if (request.ProfileId.HasValue)
            query = query.Where(e => e.PrimaryProfileId == request.ProfileId.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(term) ||
                e.LastName.ToLower().Contains(term) ||
                e.Email.ToLower().Contains(term) ||
                e.EmployeeCode.ToLower().Contains(term)
            );
        }

        var employees = await query.ToListAsync(cancellationToken);

        return employees.Select(e => MapToDto(e)).ToList();
    }

    private EmployeeDto MapToDto(Domain.Entities.Employee e)
    {
        return new EmployeeDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            EmployeeCode = e.EmployeeCode,
            JoinedDate = e.JoinedDate,
            BankName = e.BankName,
            BankAccountNumber = e.BankAccountNumber,
            BankBranchCode = e.BankBranchCode,
            CompanyId = e.CompanyId,
            CompanyName = e.Company?.Name ?? string.Empty,
            DepartmentId = e.DepartmentId,
            DepartmentName = e.Department?.Name ?? string.Empty,
            PrimaryProfileId = e.PrimaryProfileId,
            ProfileName = e.PrimaryProfile?.Name ?? string.Empty,
            IsActive = e.IsActive,
            SalaryStructures = e.EmployeeSalaryStructures?.Select(s => new SalaryStructureDto
            {
                Id = s.Id,
                Currency = s.Currency,
                Amount = s.Amount,
                EffectiveStartDate = s.EffectiveStartDate,
                EffectiveEndDate = s.EffectiveEndDate
            }).ToList() ?? new List<SalaryStructureDto>(),
            CustomBenefits = e.EmployeeCustomBenefits?.Select(b => new CustomBenefitDto
            {
                Id = b.Id,
                BenefitName = b.BenefitName,
                Amount = b.Amount,
                Currency = b.Currency,
                IsRecurring = b.IsRecurring
            }).ToList() ?? new List<CustomBenefitDto>()
        };
    }
}