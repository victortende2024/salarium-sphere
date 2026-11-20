using MediatR;

namespace SalariumSphere.Application.Features.Companies.Commands;

public class DeleteCompanyCommand : IRequest
{
    public required Guid Id { get; set; }
}