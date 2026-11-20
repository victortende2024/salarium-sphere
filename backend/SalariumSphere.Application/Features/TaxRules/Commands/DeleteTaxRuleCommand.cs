using MediatR;

namespace SalariumSphere.Application.Features.TaxRules.Commands;

public class DeleteTaxRuleCommand : IRequest
{
    public required Guid Id { get; set; }
}