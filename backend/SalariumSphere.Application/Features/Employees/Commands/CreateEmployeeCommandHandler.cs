using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.Employees.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Employees.Commands;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public CreateEmployeeCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<EmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        // Get current tenant
        var tenant = await _tenantResolver.GetCurrentTenantAsync();

        // After getting the tenant
        var company = await _context.Set<Company>()
            .FirstOrDefaultAsync(c => c.Id == request.CompanyId && c.TenantId == tenant.Id && c.IsActive, cancellationToken);

        if (company == null)
            throw new Exception($"Company with ID {request.CompanyId} not found or does not belong to this tenant.");

        // Check for duplicate email
        var existing = await _context.Set<Employee>()
            .AnyAsync(e => e.Email == request.Email && e.TenantId == tenant.Id, cancellationToken);
        if (existing)
            throw new Exception($"Employee with email '{request.Email}' already exists.");

        // Auto-generate EmployeeCode if not provided
        var employeeCode = request.EmployeeCode;
        if (string.IsNullOrWhiteSpace(employeeCode))
        {
            // Get the max code for this tenant
            var maxCode = await _context.Set<Employee>()
                .Where(e => e.TenantId == tenant.Id)
                .Select(e => e.EmployeeCode)
                .ToListAsync(cancellationToken);
            var nextNumber = maxCode
                .Where(c => c.StartsWith("EMP-"))
                .Select(c => int.TryParse(c.Replace("EMP-", ""), out var num) ? num : 0)
                .DefaultIfEmpty(0)
                .Max() + 1;
            employeeCode = $"EMP-{nextNumber:D3}";
        }

        // Create employee
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            EmployeeCode = employeeCode,
            JoinedDate = request.JoinedDate,
            BankName = request.BankName,
            BankAccountNumber = request.BankAccountNumber,
            BankBranchCode = request.BankBranchCode,
            CompanyId = request.CompanyId,
            DepartmentId = request.DepartmentId,
            PrimaryProfileId = request.PrimaryProfileId,
            TenantId = tenant.Id,
            IsActive = true
        };

        // Add salary structures
        foreach (var salaryDto in request.SalaryStructures)
        {
            employee.EmployeeSalaryStructures.Add(new EmployeeSalaryStructure
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

        // Add custom benefits
        foreach (var benefitDto in request.CustomBenefits)
        {
            employee.EmployeeCustomBenefits.Add(new EmployeeCustomBenefit
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

        await _context.Set<Employee>().AddAsync(employee, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Return the created employee
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
            DepartmentId = employee.DepartmentId,
            PrimaryProfileId = employee.PrimaryProfileId,
            IsActive = employee.IsActive,
            SalaryStructures = employee.EmployeeSalaryStructures.Select(s => new SalaryStructureDto
            {
                Id = s.Id,
                Currency = s.Currency,
                Amount = s.Amount,
                EffectiveStartDate = s.EffectiveStartDate,
                EffectiveEndDate = s.EffectiveEndDate
            }).ToList(),
            CustomBenefits = employee.EmployeeCustomBenefits.Select(b => new CustomBenefitDto
            {
                Id = b.Id,
                BenefitName = b.BenefitName,
                Amount = b.Amount,
                Currency = b.Currency,
                IsRecurring = b.IsRecurring
            }).ToList()
        };
    }
}