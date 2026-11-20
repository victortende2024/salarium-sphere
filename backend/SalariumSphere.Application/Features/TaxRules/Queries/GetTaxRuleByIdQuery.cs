using MediatR;
using SalariumSphere.Application.Features.TaxRules.Common;

namespace SalariumSphere.Application.Features.TaxRules.Queries;

public class GetTaxRuleByIdQuery : IRequest<TaxRuleDto>
{
    public required Guid Id { get; set; }
}