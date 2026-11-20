using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.Employees.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Employees.Queries;

public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    private readonly IApplicationDbContext _context;

    public GetEmployeeByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeDto> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var employee = await _context.Set<Employee>()
            .Include(e => e.Company)
            .Include(e => e.Department)
            .Include(e => e.PrimaryProfile)
            .Include(e => e.EmployeeSalaryStructures)
            .Include(e => e.EmployeeCustomBenefits)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (employee == null)
            throw new Exception($"Employee with ID {request.Id} was not found.");

        return MapToDto(employee);
    }

    private EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            EmployeeCode = employee.EmployeeCode,
            JoinedDate = employee.JoinedDate,
            BankName = employee.BankName,
            BankAccountNumber = employee.BankAccountNumber,
            BankBranchCode = employee.BankBranchCode,
            CompanyId = employee.CompanyId,
            CompanyName = employee.Company?.Name ?? string.Empty,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department?.Name ?? string.Empty,
            PrimaryProfileId = employee.PrimaryProfileId,
            ProfileName = employee.PrimaryProfile?.Name ?? string.Empty,
            IsActive = employee.IsActive,
            SalaryStructures = employee.EmployeeSalaryStructures?.Select(s => new SalaryStructureDto
            {
                Id = s.Id,
                Currency = s.Currency,
                Amount = s.Amount,
                EffectiveStartDate = s.EffectiveStartDate,
                EffectiveEndDate = s.EffectiveEndDate
            }).ToList() ?? new List<SalaryStructureDto>(),
            CustomBenefits = employee.EmployeeCustomBenefits?.Select(b => new CustomBenefitDto
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