using MarcusMedina.Fluent.Decision.Core;
using MarcusMedina.Fluent.Decision.Enums;
using MarcusMedina.Fluent.Decision.Extensions;

namespace DecisionDemo;

/// <summary>
/// Comprehensive demonstration of the MarcusMedina.Decision library.
/// Shows various decision-making scenarios with different approaches and features.
/// </summary>
/// <remarks>
/// Fluent API cheat sheet — what the parameters below actually mean:
///
/// .WithCriterion(name, weight, normalization, description?)
///   name          - label shown in output, e.g. "Price".
///   weight        - this criterion's share of the total score. All weights on a
///                   decision should sum to 1.0 (e.g. 0.4 + 0.3 + 0.3).
///   normalization - how raw scores are rescaled before weighting:
///                     Linear        higher raw score = better (e.g. quality 0-10)
///                     InverseLinear lower raw score = better (e.g. price, latency)
///                     Logarithmic   for values that vary by orders of magnitude
///                     SquareRoot    milder scaling than Linear
///                     None          use the raw score as-is, no rescaling
///   description   - optional free-text note, purely for humans reading the output.
///
/// .WithOption(name, notes?)
///   name  - the thing being evaluated, e.g. "Italian Bistro".
///   notes - optional free-text note.
///
/// .WithScore(criterionName, value, confidence, notes?)
///   criterionName - must match a name passed to WithCriterion earlier.
///   value         - the raw score for this option on that criterion, on
///                   whatever scale you chose (e.g. 1-10, or actual price in €).
///   confidence    - how sure you are about this specific number:
///                     VeryLow, Low, Medium, High, VeryHigh.
///                   Doesn't affect the score itself, just recorded for later review.
///   notes         - optional free-text note.
///
/// .WithAggregation(type) - how per-criterion scores combine into one final score:
///   WeightedAverage  standard: score * weight, summed (the usual choice)
///   GeometricMean    penalizes options that are weak on any single criterion
///   HarmonicMean     penalizes very low scores even more strongly
///   MinScore         conservative: only the worst criterion counts
///   MaxScore         optimistic: only the best criterion counts
/// </remarks>
internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("🧭 MarcusMedina.Decision Demo");
        Console.WriteLine("===============================\n");

        // Demonstrate different decision scenarios
        DemonstrateBasicDecision();
        DemonstrateTechnologyChoice();
        DemonstrateCarPurchaseDecision();
        DemonstrateJobOfferComparison();
        DemonstrateTennisWeatherDecision();
        DemonstrateSensitivityAnalysis();
        DemonstrateAggregationMethods();

        Console.WriteLine("\n✨ Demo completed! Press any key to exit...");
        Console.ReadKey();
    }

    private static void DemonstrateBasicDecision()
    {
        Console.WriteLine("📊 Basic Decision: Restaurant Choice");
        Console.WriteLine("------------------------------------");

        var decision = Decision.Create("Choose Restaurant for Date Night")
            .WithContext("Anniversary dinner - want something special but not too expensive")
            .WithCriterion("Food Quality", 0.4, NormalizationType.Linear, "How good is the food")
            .WithCriterion("Price", 0.3, NormalizationType.InverseLinear, "Cost per person (lower is better)")
            .WithCriterion("Atmosphere", 0.3, NormalizationType.Linear, "Ambiance and romance factor")
            .WithOption("Italian Bistro")
                .WithScore("Food Quality", 9.0, ConfidenceLevel.High, "Excellent reviews")
                .WithScore("Price", 60.0, ConfidenceLevel.High, "€60 per person")
                .WithScore("Atmosphere", 8.5, ConfidenceLevel.High, "Very romantic")
            .WithOption("French Fine Dining")
                .WithScore("Food Quality", 9.5, ConfidenceLevel.High, "Michelin starred")
                .WithScore("Price", 120.0, ConfidenceLevel.High, "€120 per person")
                .WithScore("Atmosphere", 9.0, ConfidenceLevel.High, "Perfect for special occasions")
            .WithOption("Cozy Tapas")
                .WithScore("Food Quality", 7.5, ConfidenceLevel.Medium, "Good but not exceptional")
                .WithScore("Price", 35.0, ConfidenceLevel.High, "€35 per person")
                .WithScore("Atmosphere", 7.0, ConfidenceLevel.Medium, "Casual but nice")
            .BuildAndCompute();

        Console.WriteLine(decision.GetSummary());
        Console.WriteLine();
        
        var recommendation = decision.GetRecommendation();
        Console.WriteLine($"🏆 Recommendation: {recommendation?.Name}");
        Console.WriteLine($"   Score: {recommendation?.TotalScore:F3}");
        Console.WriteLine($"   Confidence: {recommendation?.OverallConfidence}");
        Console.WriteLine();
    }

    private static void DemonstrateTechnologyChoice()
    {
        Console.WriteLine("💻 Technology Decision: Cloud Provider Selection");
        Console.WriteLine("------------------------------------------------");

        var decision = Decision.Create("Select Cloud Provider for E-commerce Platform")
            .WithContext("Migrating legacy e-commerce platform to cloud - need reliability and performance")
            .WithCriterion("Performance", 0.35, NormalizationType.Linear, "Response time and throughput")
            .WithCriterion("Cost", 0.25, NormalizationType.InverseLinear, "Monthly operational cost")
            .WithCriterion("Reliability", 0.25, NormalizationType.Linear, "Uptime SLA and disaster recovery")
            .WithCriterion("Security", 0.15, NormalizationType.Linear, "Security features and compliance")
            .WithOption("AWS")
                .WithScore("Performance", 9.0, ConfidenceLevel.High, "Proven high performance")
                .WithScore("Cost", 7.0, ConfidenceLevel.High, "Premium pricing but predictable")
                .WithScore("Reliability", 9.5, ConfidenceLevel.High, "Industry-leading 99.99% SLA")
                .WithScore("Security", 9.0, ConfidenceLevel.High, "Comprehensive security suite")
            .WithOption("Azure")
                .WithScore("Performance", 8.5, ConfidenceLevel.High, "Excellent performance")
                .WithScore("Cost", 7.5, ConfidenceLevel.Medium, "Competitive pricing")
                .WithScore("Reliability", 9.0, ConfidenceLevel.High, "99.9% SLA")
                .WithScore("Security", 8.5, ConfidenceLevel.High, "Strong enterprise security")
            .WithOption("Google Cloud")
                .WithScore("Performance", 8.8, ConfidenceLevel.High, "Superior for data analytics")
                .WithScore("Cost", 8.0, ConfidenceLevel.Medium, "Most cost-effective")
                .WithScore("Reliability", 8.5, ConfidenceLevel.Medium, "Good but newer platform")
                .WithScore("Security", 8.0, ConfidenceLevel.Medium, "Good security, improving")
            .WithAggregation(AggregationType.WeightedAverage)
            .BuildAndCompute();

        Console.WriteLine($"Decision Analysis: {decision.Title}");
        Console.WriteLine($"Criteria weights: Performance (35%), Cost (25%), Reliability (25%), Security (15%)");
        Console.WriteLine();

        Console.WriteLine("Ranked Options:");
        foreach (var (option, rank) in decision.GetRankedOptions().Select((opt, i) => (opt, i + 1)))
        {
            Console.WriteLine($"{rank}. {option.Name}: {option.TotalScore:F3}");
            Console.WriteLine($"   Scores: {option.GetScoresSummary()}");
            Console.WriteLine($"   Overall Confidence: {option.OverallConfidence}");
        }
        Console.WriteLine();

        // Demonstrate option comparison
        var comparison = decision.Compare("AWS", "Google Cloud");
        Console.WriteLine("AWS vs Google Cloud Comparison:");
        Console.WriteLine($"Winner: {comparison.BetterOption.Name} (by {comparison.ScoreDifference:F3} points)");
        Console.WriteLine($"AWS advantages: {string.Join(", ", comparison.Option1Advantages)}");
        Console.WriteLine($"Google Cloud advantages: {string.Join(", ", comparison.Option2Advantages)}");
        Console.WriteLine();
    }

    private static void DemonstrateCarPurchaseDecision()
    {
        Console.WriteLine("🚗 Personal Decision: Car Purchase");
        Console.WriteLine("----------------------------------");

        var decision = Decision.Create("Buy Family Car")
            .WithContext("Need reliable family car for daily commute and weekend trips")
            .WithCriterion("Reliability", 0.3, NormalizationType.Linear, "Expected years without major issues")
            .WithCriterion("Fuel Economy", 0.25, NormalizationType.Linear, "Miles per gallon")
            .WithCriterion("Safety Rating", 0.25, NormalizationType.Linear, "IIHS safety score")
            .WithCriterion("Purchase Price", 0.2, NormalizationType.InverseLinear, "Total cost including fees")
            .WithOption("Toyota Camry")
                .WithScore("Reliability", 9.0, ConfidenceLevel.High, "Excellent track record")
                .WithScore("Fuel Economy", 32.0, ConfidenceLevel.High, "32 MPG combined")
                .WithScore("Safety Rating", 9.5, ConfidenceLevel.High, "Top Safety Pick+")
                .WithScore("Purchase Price", 28000.0, ConfidenceLevel.High, "$28,000")
            .WithOption("Honda Accord")
                .WithScore("Reliability", 8.8, ConfidenceLevel.High, "Very reliable")
                .WithScore("Fuel Economy", 33.0, ConfidenceLevel.High, "33 MPG combined")
                .WithScore("Safety Rating", 9.0, ConfidenceLevel.High, "Top Safety Pick")
                .WithScore("Purchase Price", 27500.0, ConfidenceLevel.High, "$27,500")
            .WithOption("Volkswagen Passat")
                .WithScore("Reliability", 7.5, ConfidenceLevel.Medium, "Average reliability")
                .WithScore("Fuel Economy", 29.0, ConfidenceLevel.High, "29 MPG combined")
                .WithScore("Safety Rating", 8.5, ConfidenceLevel.High, "Good safety rating")
                .WithScore("Purchase Price", 25000.0, ConfidenceLevel.High, "$25,000")
            .WithAggregation(AggregationType.WeightedAverage)
            .BuildAndCompute();

        Console.WriteLine($"Analysis: {decision.Title}");
        var analysis = decision.Analyze();
        
        Console.WriteLine($"Recommendation: {analysis.Recommendation?.Name ?? "None"}");
        Console.WriteLine($"Clear winner: {(analysis.HasClearWinner ? "Yes" : "No")}");
        Console.WriteLine($"Confidence: {analysis.RecommendationConfidence}");
        Console.WriteLine();

        Console.WriteLine("Detailed Rankings:");
        foreach (var option in analysis.RankedOptions)
        {
            Console.WriteLine($"• {option.ToString("F", null)}");
        }
        Console.WriteLine();
    }

    private static void DemonstrateJobOfferComparison()
    {
        Console.WriteLine("💼 Career Decision: Job Offer Comparison");
        Console.WriteLine("----------------------------------------");

        var decision = Decision.Create("Choose Job Offer")
            .WithContext("Comparing three job offers - looking for career growth and work-life balance")
            .WithCriterion("Salary", 0.25, NormalizationType.Linear, "Annual base salary")
            .WithCriterion("Growth Potential", 0.3, NormalizationType.Linear, "Career advancement opportunities")
            .WithCriterion("Work-Life Balance", 0.25, NormalizationType.Linear, "Flexibility and reasonable hours")
            .WithCriterion("Company Culture", 0.2, NormalizationType.Linear, "Team dynamics and values alignment")
            .WithOption("Tech Startup")
                .WithScore("Salary", 95000.0, ConfidenceLevel.High, "$95k base")
                .WithScore("Growth Potential", 9.0, ConfidenceLevel.Medium, "Rapid growth if successful")
                .WithScore("Work-Life Balance", 6.0, ConfidenceLevel.High, "Long hours expected")
                .WithScore("Company Culture", 8.5, ConfidenceLevel.Medium, "Young, energetic team")
            .WithOption("Fortune 500")
                .WithScore("Salary", 110000.0, ConfidenceLevel.High, "$110k base + bonus")
                .WithScore("Growth Potential", 7.0, ConfidenceLevel.High, "Structured advancement")
                .WithScore("Work-Life Balance", 8.0, ConfidenceLevel.High, "Good policies")
                .WithScore("Company Culture", 7.0, ConfidenceLevel.Medium, "Corporate environment")
            .WithOption("Mid-size Company")
                .WithScore("Salary", 85000.0, ConfidenceLevel.High, "$85k base")
                .WithScore("Growth Potential", 8.0, ConfidenceLevel.Medium, "Good opportunities")
                .WithScore("Work-Life Balance", 9.0, ConfidenceLevel.High, "Excellent balance")
                .WithScore("Company Culture", 9.0, ConfidenceLevel.High, "Great team fit")
            .WithAggregation(AggregationType.WeightedAverage)
            .BuildAndCompute();

        Console.WriteLine(decision.GenerateReport(includeDetails: false));
        Console.WriteLine();

        // Show validation
        var errors = decision.GetValidationErrors().ToList();
        if (errors.Any())
        {
            Console.WriteLine("⚠️ Validation Issues:");
            foreach (var error in errors)
                Console.WriteLine($"   • {error}");
        }
        else
        {
            Console.WriteLine("✅ Decision validation passed");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// The classic "Play Tennis" dataset (Outlook / Temperature / Humidity / Wind)
    /// is a staple of decision TREE teaching (Mitchell's ID3 example) — it classifies
    /// a single day as Yes/No by splitting on categorical attributes.
    ///
    /// This library builds decision MATRICES, not trees: it ranks a set of options
    /// against weighted, numeric criteria — a different technique for a different
    /// kind of question. So instead of pretending to reproduce a tree split, this
    /// borrows the same four weather attributes and asks the matrix's actual
    /// question: "given how each day's weather scores, which day is the better
    /// choice to play?" Two days are lifted from the textbook table (day 3, a
    /// classic "Yes", and day 6, a classic "No") to keep the comparison honest.
    /// </summary>
    private static void DemonstrateTennisWeatherDecision()
    {
        Console.WriteLine("🎾 Weather Decision: Best Day to Play Tennis");
        Console.WriteLine("---------------------------------------------");

        var decision = Decision.Create("Which Day Should I Play Tennis?")
            .WithContext("Comparing two forecast days using the classic Outlook/Temperature/Humidity/Wind attributes")
            .WithCriterion("Outlook", 0.30, NormalizationType.Linear, "Sky condition: clearer/overcast scores higher, rain scores low")
            .WithCriterion("Temperature", 0.20, NormalizationType.Linear, "Comfort while playing")
            .WithCriterion("Humidity", 0.25, NormalizationType.InverseLinear, "Raw humidity level - lower is more comfortable")
            .WithCriterion("Wind", 0.25, NormalizationType.InverseLinear, "Raw wind strength - calmer is easier to play in")
            .WithOption("Wednesday (Overcast, Hot, High Humidity, Weak Wind)")
                .WithScore("Outlook", 9.0, ConfidenceLevel.High, "Overcast - no glare, no rain")
                .WithScore("Temperature", 6.0, ConfidenceLevel.Medium, "Hot but tolerable")
                .WithScore("Humidity", 8.0, ConfidenceLevel.High, "High humidity")
                .WithScore("Wind", 2.0, ConfidenceLevel.High, "Weak wind - easy rallies")
            .WithOption("Thursday (Rain, Cool, Normal Humidity, Strong Wind)")
                .WithScore("Outlook", 2.0, ConfidenceLevel.High, "Rain - court likely wet")
                .WithScore("Temperature", 7.0, ConfidenceLevel.Medium, "Cool and comfortable")
                .WithScore("Humidity", 3.0, ConfidenceLevel.High, "Normal humidity")
                .WithScore("Wind", 8.0, ConfidenceLevel.High, "Strong wind - hard to control shots")
            .WithAggregation(AggregationType.WeightedAverage)
            .BuildAndCompute();

        Console.WriteLine(decision.GenerateReport(includeDetails: false));
        Console.WriteLine();
    }

    private static void DemonstrateSensitivityAnalysis()
    {
        Console.WriteLine("📈 Advanced Analysis: Sensitivity Testing");
        Console.WriteLine("----------------------------------------");

        var decision = Decision.Create("Software Architecture Decision")
            .WithCriterion("Performance", 0.4, NormalizationType.Linear)
            .WithCriterion("Maintainability", 0.3, NormalizationType.Linear)
            .WithCriterion("Cost", 0.3, NormalizationType.InverseLinear)
            .WithOption("Microservices")
                .WithScore("Performance", 8.0, ConfidenceLevel.Medium)
                .WithScore("Maintainability", 9.0, ConfidenceLevel.High)
                .WithScore("Cost", 6.0, ConfidenceLevel.Medium)
            .WithOption("Monolith")
                .WithScore("Performance", 9.0, ConfidenceLevel.High)
                .WithScore("Maintainability", 7.0, ConfidenceLevel.High)
                .WithScore("Cost", 8.0, ConfidenceLevel.High)
            .WithOption("Modular Monolith")
                .WithScore("Performance", 8.5, ConfidenceLevel.High)
                .WithScore("Maintainability", 8.0, ConfidenceLevel.Medium)
                .WithScore("Cost", 7.5, ConfidenceLevel.Medium)
            .BuildAndCompute();

        Console.WriteLine($"Base recommendation: {decision.GetRecommendation()?.Name}");
        Console.WriteLine();

        // Perform sensitivity analysis on Performance weight
        var sensitivity = decision.PerformSensitivityAnalysis("Performance", (0.2, 0.6), steps: 5);
        
        Console.WriteLine("Sensitivity Analysis - Performance Weight Impact:");
        Console.WriteLine($"Is recommendation stable: {(sensitivity.IsStable ? "Yes" : "No")}");
        Console.WriteLine();
        
        Console.WriteLine("Weight variation results:");
        foreach (var (weight, winner, score) in sensitivity.Results)
        {
            Console.WriteLine($"Performance weight {weight:P0}: {winner} wins (score: {score:F3})");
        }
        Console.WriteLine();

        Console.WriteLine("Optimal weight ranges:");
        foreach (var (option, minWeight, maxWeight) in sensitivity.OptimalRanges)
        {
            Console.WriteLine($"{option}: optimal when Performance weight is {minWeight:P0} - {maxWeight:P0}");
        }
        Console.WriteLine();
    }

    private static void DemonstrateAggregationMethods()
    {
        Console.WriteLine("⚖️ Aggregation Methods Comparison");
        Console.WriteLine("----------------------------------");

        var baseDecision = Decision.Create("Investment Strategy")
            .WithCriterion("Expected Return", 0.4, NormalizationType.Linear)
            .WithCriterion("Risk Level", 0.3, NormalizationType.InverseLinear)
            .WithCriterion("Liquidity", 0.3, NormalizationType.Linear)
            .WithOption("Stocks")
                .WithScore("Expected Return", 8.0, ConfidenceLevel.Medium)
                .WithScore("Risk Level", 7.0, ConfidenceLevel.High)
                .WithScore("Liquidity", 9.0, ConfidenceLevel.High)
            .WithOption("Bonds")
                .WithScore("Expected Return", 4.0, ConfidenceLevel.High)
                .WithScore("Risk Level", 3.0, ConfidenceLevel.High)
                .WithScore("Liquidity", 8.0, ConfidenceLevel.High)
            .WithOption("Real Estate")
                .WithScore("Expected Return", 6.0, ConfidenceLevel.Medium)
                .WithScore("Risk Level", 5.0, ConfidenceLevel.Medium)
                .WithScore("Liquidity", 3.0, ConfidenceLevel.High);

        var aggregationMethods = new[]
        {
            AggregationType.WeightedAverage,
            AggregationType.GeometricMean,
            AggregationType.MinScore,
            AggregationType.MaxScore,
            AggregationType.HarmonicMean
        };

        Console.WriteLine("How different aggregation methods affect the recommendation:");
        Console.WriteLine();

        foreach (var method in aggregationMethods)
        {
            var decision = baseDecision.WithAggregation(method).BuildAndCompute();
            var winner = decision.GetRecommendation();
            
            Console.WriteLine($"{method,-18}: {winner?.Name ?? "None",-12} (Score: {winner?.TotalScore ?? 0:F3})");
        }
        Console.WriteLine();

        Console.WriteLine("📋 Aggregation Method Guide:");
        Console.WriteLine("• Weighted Average: Balanced approach, most common");
        Console.WriteLine("• Geometric Mean: Prevents compensation between criteria");
        Console.WriteLine("• Min Score: Conservative, focuses on worst performance");
        Console.WriteLine("• Max Score: Optimistic, focuses on best performance");
        Console.WriteLine("• Harmonic Mean: Penalizes options with very low scores");
        Console.WriteLine();
    }
}