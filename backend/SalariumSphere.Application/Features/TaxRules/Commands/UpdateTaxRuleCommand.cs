using MediatR;
using SalariumSphere.Application.Features.TaxRules.Common;
using SalariumSphere.Domain.Entities;
using SalariumSphere.Domain.Enums;

namespace SalariumSphere.Application.Features.TaxRules.Commands;

public class UpdateTaxRuleCommand : IRequest<TaxRuleDto>
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? CountryCode { get; set; }
    public bool IsMandatory { get; set; }
    public DeductionCalculationType CalculationType { get; set; }
    public decimal? FixedAmount { get; set; }
    public CurrencyCode? FixedAmountCurrency { get; set; }
    public decimal? PercentageValue { get; set; }
    public string? TieredConfigJson { get; set; }
    public decimal? CapAmount { get; set; }
    public CurrencyCode? CapCurrency { get; set; }
    public decimal? ThresholdAmount { get; set; }
    public CurrencyCode? ThresholdCurrency { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
}