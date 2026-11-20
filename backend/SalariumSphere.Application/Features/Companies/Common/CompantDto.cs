using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Companies.Common;

public class CompanyDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Guid? ParentCompanyId { get; set; }
    public string? ParentCompanyName { get; set; }
    public bool IsActive { get; set; }
    public List<DepartmentDto> Departments { get; set; } = new();
}

public class DepartmentDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Guid CompanyId { get; set; }
    public bool IsActive { get; set; }
}