using SalariumSphere.Domain.Entities;
using SalariumSphere.Domain.Enums;

namespace SalariumSphere.Application.Common;

public static class DeductionCalculator
{
    public static decimal CalculateDeduction(
        decimal grossPay,
        CurrencyCode baseCurrency,
        DeductionCalculationType calculationType,
        decimal? fixedAmount = null,
        decimal? percentageValue = null,
        string? tieredConfigJson = null,
        decimal? capAmount = null,
        decimal? thresholdAmount = null)
    {
        decimal result = 0;

        switch (calculationType)
        {
            case DeductionCalculationType.Fixed:
                result = fixedAmount ?? 0;
                if (capAmount.HasValue && result > capAmount.Value)
                    result = capAmount.Value;
                break;

            case DeductionCalculationType.Percentage:
                result = grossPay * (percentageValue ?? 0) / 100;
                if (thresholdAmount.HasValue && thresholdAmount.Value > 0)
                {
                    decimal excessAmount = grossPay - thresholdAmount.Value;
                    result = Math.Max(0, (excessAmount * (percentageValue ?? 0) / 100));
                }
                if (capAmount.HasValue && result > capAmount.Value)
                    result = capAmount.Value;
                break;

            case DeductionCalculationType.Tiered:
                result = CalculateTieredDeduction(grossPay, tieredConfigJson);
                Console.WriteLine($"Tiered deduction for gross {grossPay}: {result}");
                if (capAmount.HasValue && result > capAmount.Value)
                    result = capAmount.Value;
                break;
        }

        return Math.Max(0, result);
    }

    private static decimal CalculateTieredDeduction(decimal grossPay, string? tieredConfigJson)
    {
        if (string.IsNullOrWhiteSpace(tieredConfigJson))
        {
            Console.WriteLine($"TieredConfigJson is null or empty.");
            return 0;
        }

        Console.WriteLine($"Processing tiered JSON: {tieredConfigJson}");

        try
        {
            // Use case-insensitive deserialization
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Try to deserialize as List<Tier>
            var tiers = System.Text.Json.JsonSerializer.Deserialize<List<Tier>>(tieredConfigJson, options);

            if (tiers == null || !tiers.Any())
            {
                Console.WriteLine($"Deserialization returned null or empty list.");
                // Try to see if it's an object with a "tiers" property
                try
                {
                    var wrapper = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<Tier>>>(tieredConfigJson, options);
                    if (wrapper != null && wrapper.TryGetValue("tiers", out var wrappedTiers))
                    {
                        tiers = wrappedTiers;
                        Console.WriteLine($"Found tiers inside 'tiers' property.");
                    }
                }
                catch { }
            }

            if (tiers == null || !tiers.Any())
            {
                Console.WriteLine($"Still no tiers found after fallback.");
                return 0;
            }

            Console.WriteLine($"Parsed {tiers.Count} tiers.");

            decimal deduction = 0;
            decimal remainingGross = grossPay;

            // Sort tiers by StartBand
            foreach (var tier in tiers.OrderBy(t => t.StartBand))
            {
                Console.WriteLine($"Tier: Start={tier.StartBand}, End={tier.EndBand}, Rate={tier.Rate}%");

                if (remainingGross <= 0)
                    break;

                // If gross is below the start band, we break because all subsequent tiers have higher start bands
                if (grossPay <= tier.StartBand)
                    break;

                // Calculate amount in this tier
                decimal tierAmount;
                if (tier.EndBand.HasValue)
                {
                    // The amount is the difference between the end and start, but capped by remaining gross
                    decimal bandWidth = tier.EndBand.Value - tier.StartBand;
                    tierAmount = Math.Min(remainingGross, bandWidth);
                    if (tierAmount < 0) tierAmount = 0;
                }
                else
                {
                    // No end band – take all remaining gross
                    tierAmount = remainingGross;
                }

                deduction += tierAmount * (tier.Rate / 100);
                remainingGross -= tierAmount;

                Console.WriteLine($"  -> TierAmount: {tierAmount}, Deduction so far: {deduction}");
            }

            Console.WriteLine($"Final deduction: {deduction}");
            return deduction;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CalculateTieredDeduction: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return 0;
        }
    }

    private class Tier
    {
        public decimal StartBand { get; set; }
        public decimal? EndBand { get; set; }
        public decimal Rate { get; set; }
    }
}