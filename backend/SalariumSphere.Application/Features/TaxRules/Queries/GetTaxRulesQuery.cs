using MediatR;
using SalariumSphere.Application.Features.TaxRules.Common;

namespace SalariumSphere.Application.Features.TaxRules.Queries;

public class GetTaxRulesQuery : IRequest<List<TaxRuleDto>> { }