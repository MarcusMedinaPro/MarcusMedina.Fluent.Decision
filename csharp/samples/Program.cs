using MarcusMedina.Fluent.Decision.Core;
using MarcusMedina.Fluent.Decision.Enums;
using MarcusMedina.Fluent.Decision.Extensions;

namespace DecisionDemo;

/// <summary>
/// Comprehensive demonstration of the MarcusMedina.Decision library.
/// Shows various decision-making scenarios with different approaches and features.
/// </summary>
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