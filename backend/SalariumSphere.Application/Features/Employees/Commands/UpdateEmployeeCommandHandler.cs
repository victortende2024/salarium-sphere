using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.Employees.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Employees.Commands;

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, EmployeeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public UpdateEmployeeCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<EmployeeDto> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();

        // Fetch employee without children
        var employee = await _context.Set<Employee>()
            .FirstOrDefaultAsync(e => e.Id == request.Id && e.TenantId == tenant.Id && e.IsActive, cancellationToken);

        if (employee == null)
            throw new Exception($"Employee with ID {request.Id} not found.");

        // Update basic properties
        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.Email = request.Email;
        employee.EmployeeCode = string.IsNullOrWhiteSpace(request.EmployeeCode) ? employee.EmployeeCode : request.EmployeeCode;
        employee.JoinedDate = request.JoinedDate;
        employee.BankName = request.BankName;
        employee.BankAccountNumber = request.BankAccountNumber;
        employee.BankBranchCode = request.BankBranchCode;
        employee.CompanyId = request.CompanyId;
        employee.DepartmentId = request.DepartmentId;
        employee.PrimaryProfileId = request.PrimaryProfileId;
        employee.UpdatedAtUtc = DateTime.UtcNow;

        // Mark the entity as modified using Update
        _context.Set<Employee>().Update(employee);

        // Handle salary structures: delete existing, add new ones
        var existingStructures = await _context.Set<EmployeeSalaryStructure>()
            .Where(s => s.EmployeeId == employee.Id)
            .ToListAsync(cancellationToken);
        _context.Set<EmployeeSalaryStructure>().RemoveRange(existingStructures);

        foreach (var salaryDto in request.SalaryStructures)
        {
            _context.Set<EmployeeSalaryStructure>().Add(new EmployeeSalaryStructure
            {
                Id = Guid.NewGuid(),
                EmployeeId = employee.Id,
                Currency = salaryDto.Currency,
                Amount = salaryDto.Amount,
                EffectiveStartDate = salaryDto.EffectiveStartDate,
                EffectiveEndDate = salaryDto.EffectiveEndDate,
                TenantId = tenant.Id,
                IsActive = true
            });
        }

        // Handle custom benefits similarly
        var existingBenefits = await _context.Set<EmployeeCustomBenefit>()
            .Where(b => b.EmployeeId == employee.Id)
            .ToListAsync(cancellationToken);
        _context.Set<EmployeeCustomBenefit>().RemoveRange(existingBenefits);

        foreach (var benefitDto in request.CustomBenefits)
        {
            _context.Set<EmployeeCustomBenefit>().Add(new EmployeeCustomBenefit
            {
                Id = Guid.NewGuid(),
                EmployeeId = employee.Id,
                BenefitName = benefitDto.BenefitName,
                Amount = benefitDto.Amount,
                Currency = benefitDto.Currency,
                IsRecurring = benefitDto.IsRecurring,
                TenantId = tenant.Id,
                IsActive = true
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Re-fetch updated employee with navigation properties for the DTO
        var updated = await _context.Set<Employee>()
            .Include(e => e.Company)
            .Include(e => e.Department)
            .Include(e => e.PrimaryProfile)
            .Include(e => e.EmployeeSalaryStructures)
            .Include(e => e.EmployeeCustomBenefits)
            .FirstAsync(e => e.Id == employee.Id, cancellationToken);

        return MapToDto(updated);
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