using MarcusMedina.Fluent.Decision.Builders;
using MarcusMedina.Fluent.Decision.Core;
using MarcusMedina.Fluent.Decision.Enums;

namespace MarcusMedina.Fluent.Decision.Extensions;

/// <summary>
/// Extension methods for the Decision fluent API.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Purpose:</strong> Provides convenient static entry points and extension methods to make the decision API more discoverable and easier to use.
/// </para>
/// <para>
/// <strong>When to use:</strong> When you want the most convenient and readable way to start building decisions.
/// </para>
/// <para>
/// <strong>Example:</strong>
/// <code>
/// using MarcusMedina.Fluent.Decision.Extensions;
/// 
/// var decision = Decision.Create("Technology Choice")
///     .WithCriterion("Cost", 0.3)
///     .WithCriterion("Performance", 0.7)
///     .WithOption("Solution A")
///         .WithScore("Cost", 8.0)
///         .WithScore("Performance", 7.5)
///     .WithOption("Solution B")
///         .WithScore("Cost", 6.0)
///         .WithScore("Performance", 9.0)
///     .BuildAndCompute();
/// 
/// Console.WriteLine(decision.GetRecommendation()?.Name);
/// </code>
/// </para>
/// </remarks>
public static class DecisionExtensions
{
    /// <summary>
    /// Creates a new DecisionBuilder with the specified title.
    /// </summary>
    /// <param name="_">The Decision type (used for extension method syntax).</param>
    /// <param name="title">The title of the decision.</param>
    /// <returns>A new DecisionBuilder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when title is null or empty.</exception>
    public static DecisionBuilder Create(this Type _, string title)
    {
        return new DecisionBuilder(title);
    }

    /// <summary>
    /// Analyzes the decision by computing scores and returning analysis results.
    /// </summary>
    /// <param name="decision">The decision to analyze.</param>
    /// <returns>A decision analysis result with computed scores and recommendations.</returns>
    public static DecisionAnalysis Analyze(this Core.Decision decision)
    {
        var computed = decision.IsComputed ? decision : decision.Compute();
        return new DecisionAnalysis(computed);
    }

    /// <summary>
    /// Gets a formatted report of the decision analysis.
    /// </summary>
    /// <param name="decision">The decision to generate a report for.</param>
    /// <param name="includeDetails">Whether to include detailed scoring information.</param>
    /// <returns>A formatted analysis report.</returns>
    public static string GenerateReport(this Core.Decision decision, bool includeDetails = true)
    {
        return includeDetails ? decision.GetFullReport() : decision.GetSummary();
    }

    /// <summary>
    /// Determines if a decision is ready for computation.
    /// </summary>
    /// <param name="decision">The decision to check.</param>
    /// <returns>True if the decision has criteria, options, and all required scores.</returns>
    public static bool IsReadyForComputation(this Core.Decision decision)
    {
        if (!decision.Criteria.Any() || !decision.Options.Any())
            return false;

        return decision.Options.All(option =>
            decision.Criteria.All(criterion =>
                option.HasScore(criterion.Name)));
    }

    /// <summary>
    /// Gets validation errors for a decision.
    /// </summary>
    /// <param name="decision">The decision to validate.</param>
    /// <returns>A list of validation error messages.</returns>
    public static IEnumerable<string> GetValidationErrors(this Core.Decision decision)
    {
        var errors = new List<string>();

        if (!decision.Criteria.Any())
            errors.Add("No criteria defined");

        if (!decision.Options.Any())
            errors.Add("No options defined");

        // Check weight sum
        var totalWeight = decision.Criteria.Sum(c => c.Weight);
        if (Math.Abs(totalWeight - 1.0) > 0.001)
            errors.Add($"Criterion weights sum to {totalWeight:F3}, should be 1.0");

        // Check for missing scores
        foreach (var option in decision.Options)
        {
            foreach (var criterion in decision.Criteria)
            {
                if (!option.HasScore(criterion.Name))
                    errors.Add($"Option '{option.Name}' missing score for '{criterion.Name}'");
            }
        }

        return errors;
    }

    /// <summary>
    /// Gets the top N options ranked by score.
    /// </summary>
    /// <param name="decision">The decision to get top options from.</param>
    /// <param name="count">The number of top options to return.</param>
    /// <returns>The top N options by score.</returns>
    public static IEnumerable<Option> GetTopOptions(this Core.Decision decision, int count)
    {
        return decision.GetRankedOptions().Take(Math.Max(1, count));
    }

    /// <summary>
    /// Compares two options within a decision context.
    /// </summary>
    /// <param name="decision">The decision context.</param>
    /// <param name="option1Name">Name of the first option.</param>
    /// <param name="option2Name">Name of the second option.</param>
    /// <returns>Comparison result showing which option is better and why.</returns>
    public static OptionComparison Compare(this Core.Decision decision, string option1Name, string option2Name)
    {
        var computed = decision.IsComputed ? decision : decision.Compute();
        var option1 = computed.GetOption(option1Name);
        var option2 = computed.GetOption(option2Name);

        if (option1 == null) throw new ArgumentException($"Option '{option1Name}' not found.", nameof(option1Name));
        if (option2 == null) throw new ArgumentException($"Option '{option2Name}' not found.", nameof(option2Name));

        return new OptionComparison(option1, option2, computed.Criteria);
    }

    /// <summary>
    /// Performs sensitivity analysis by varying criterion weights.
    /// </summary>
    /// <param name="decision">The decision to analyze.</param>
    /// <param name="criterionName">The criterion to vary.</param>
    /// <param name="weightRange">The range of weights to test (0.0 to 1.0).</param>
    /// <param name="steps">The number of steps in the analysis.</param>
    /// <returns>Sensitivity analysis results.</returns>
    public static SensitivityAnalysis PerformSensitivityAnalysis(
        this Core.Decision decision, 
        string criterionName, 
        (double min, double max) weightRange, 
        int steps = 10)
    {
        var results = new List<(double weight, string winner, double score)>();
        var criterion = decision.GetCriterion(criterionName);
        
        if (criterion == null)
            throw new ArgumentException($"Criterion '{criterionName}' not found.", nameof(criterionName));

        var stepSize = (weightRange.max - weightRange.min) / Math.Max(1, steps - 1);
        
        for (int i = 0; i < steps; i++)
        {
            var testWeight = weightRange.min + (i * stepSize);
            
            // Adjust other weights proportionally
            var otherCriteria = decision.Criteria.Where(c => c.Name != criterionName).ToList();
            var remainingWeight = 1.0 - testWeight;
            var totalOtherWeight = otherCriteria.Sum(c => c.Weight);
            
            var adjustedCriteria = decision.Criteria.Select(c =>
            {
                if (c.Name == criterionName)
                    return c with { Weight = testWeight };
                else
                    return c with { Weight = totalOtherWeight > 0 ? (c.Weight / totalOtherWeight) * remainingWeight : 0 };
            }).ToList();

            // Create test decision
            var testDecision = decision with { Criteria = adjustedCriteria.AsReadOnly() };
            var computed = testDecision.Compute();
            var winner = computed.GetRecommendation();
            
            results.Add((testWeight, winner?.Name ?? "None", winner?.TotalScore ?? 0));
        }

        return new SensitivityAnalysis(criterionName, results);
    }
}

/// <summary>
/// Represents the results of a decision analysis.
/// </summary>
public sealed record DecisionAnalysis(Core.Decision Decision)
{
    /// <summary>Gets the recommended option.</summary>
    public Option? Recommendation => Decision.GetRecommendation();
    
    /// <summary>Gets all options ranked by score.</summary>
    public IEnumerable<Option> RankedOptions => Decision.GetRankedOptions();
    
    /// <summary>Gets the confidence score of the recommendation.</summary>
    public ConfidenceLevel RecommendationConfidence => Recommendation?.OverallConfidence ?? ConfidenceLevel.Medium;
    
    /// <summary>Gets whether there's a clear winner (significant score gap).</summary>
    public bool HasClearWinner
    {
        get
        {
            var ranked = RankedOptions.Take(2).ToList();
            if (ranked.Count < 2) return true;
            return ranked[0].TotalScore - ranked[1].TotalScore > 0.1; // 10% gap
        }
    }
}

/// <summary>
/// Represents a comparison between two options.
/// </summary>
public sealed record OptionComparison(Option Option1, Option Option2, IEnumerable<Criterion> Criteria)
{
    /// <summary>Gets which option is better overall.</summary>
    public Option BetterOption => Option1.TotalScore >= Option2.TotalScore ? Option1 : Option2;
    
    /// <summary>Gets the score difference.</summary>
    public double ScoreDifference => Math.Abs(Option1.TotalScore - Option2.TotalScore);
    
    /// <summary>Gets criteria where Option1 is better.</summary>
    public IEnumerable<string> Option1Advantages => GetAdvantages(Option1, Option2);
    
    /// <summary>Gets criteria where Option2 is better.</summary>
    public IEnumerable<string> Option2Advantages => GetAdvantages(Option2, Option1);
    
    private IEnumerable<string> GetAdvantages(Option option1, Option option2)
    {
        return Criteria
            .Where(c => option1.GetScore(c.Name)?.NormalizedValue > option2.GetScore(c.Name)?.NormalizedValue)
            .Select(c => c.Name);
    }
}

/// <summary>
/// Represents the results of sensitivity analysis.
/// </summary>
public sealed record SensitivityAnalysis(string CriterionName, IReadOnlyList<(double weight, string winner, double score)> Results)
{
    /// <summary>Gets whether the recommendation is stable across weight variations.</summary>
    public bool IsStable
    {
        get
        {
            var winners = Results.Select(r => r.winner).Distinct().ToList();
            return winners.Count == 1;
        }
    }
    
    /// <summary>Gets the weight ranges where each option is optimal.</summary>
    public IEnumerable<(string option, double minWeight, double maxWeight)> OptimalRanges
    {
        get
        {
            return Results
                .GroupBy(r => r.winner)
                .Select(g => (
                    option: g.Key,
                    minWeight: g.Min(r => r.weight),
                    maxWeight: g.Max(r => r.weight)
                ));
        }
    }
}