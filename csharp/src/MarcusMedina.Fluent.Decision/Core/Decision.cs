using MarcusMedina.Fluent.Decision.Enums;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MarcusMedina.Fluent.Decision.Core;

/// <summary>
/// Represents a complete decision analysis with criteria, options, and computed recommendations.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Purpose:</strong> The main orchestrator that combines criteria, options, and aggregation logic to produce ranked decision recommendations with transparency and auditability.
/// </para>
/// <para>
/// <strong>When to use:</strong> When you need to make a complex decision involving multiple factors and want a structured, repeatable approach with clear reasoning.
/// </para>
/// <para>
/// <strong>Example:</strong>
/// <code>
/// var decision = Decision.Create("Choose Development Framework")
///     .WithCriterion("Performance", 0.4, NormalizationType.Linear)
///     .WithCriterion("Cost", 0.3, NormalizationType.InverseLinear)
///     .WithCriterion("Community", 0.3, NormalizationType.Linear)
///     .WithOption("React")
///         .WithScore("Performance", 8.5)
///         .WithScore("Cost", 9.0)
///         .WithScore("Community", 9.5)
///     .WithOption("Vue")
///         .WithScore("Performance", 8.0)
///         .WithScore("Cost", 9.5)
///         .WithScore("Community", 7.5)
///     .WithAggregation(AggregationType.WeightedAverage)
///     .Build();
/// 
/// var recommendation = decision.GetRecommendation();
/// Console.WriteLine($"Recommended: {recommendation.Name} (Score: {recommendation.TotalScore:F3})");
/// </code>
/// </para>
/// </remarks>
public sealed record Decision : IFormattable, IEquatable<Decision>, IEnumerable<Option>
{
    /// <summary>Gets the title/name of this decision.</summary>
    public string Title { get; init; }
    
    /// <summary>Gets the criteria used for evaluation.</summary>
    public ReadOnlyCollection<Criterion> Criteria { get; init; }
    
    /// <summary>Gets the options being evaluated.</summary>
    public ReadOnlyCollection<Option> Options { get; init; }
    
    /// <summary>Gets the aggregation method used.</summary>
    public AggregationType Aggregation { get; init; }
    
    /// <summary>Gets optional decision context or notes.</summary>
    public string? Context { get; init; }
    
    /// <summary>Gets the timestamp when this decision was created.</summary>
    public DateTimeOffset CreatedAt { get; init; }
    
    /// <summary>Gets whether the decision has been computed.</summary>
    public bool IsComputed { get; init; }

    /// <summary>
    /// Initializes a new instance of the Decision record.
    /// </summary>
    /// <param name="title">The title of the decision.</param>
    /// <param name="criteria">The evaluation criteria.</param>
    /// <param name="options">The options to evaluate.</param>
    /// <param name="aggregation">The aggregation method.</param>
    /// <param name="context">Optional decision context.</param>
    /// <param name="createdAt">Creation timestamp.</param>
    /// <param name="isComputed">Whether scores have been computed.</param>
    /// <exception cref="ArgumentException">Thrown when title is null or empty.</exception>
    public Decision(string title,
                    IEnumerable<Criterion>? criteria = null,
                    IEnumerable<Option>? options = null,
                    AggregationType aggregation = AggregationType.WeightedAverage,
                    string? context = null,
                    DateTimeOffset? createdAt = null,
                    bool isComputed = false)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Decision title cannot be null or empty.", nameof(title));

        Title = title;
        Criteria = new ReadOnlyCollection<Criterion>((criteria ?? Enumerable.Empty<Criterion>()).ToList());
        Options = new ReadOnlyCollection<Option>((options ?? Enumerable.Empty<Option>()).ToList());
        Aggregation = aggregation;
        Context = context;
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
        IsComputed = isComputed;
    }

    /// <summary>
    /// Computes normalized scores and total scores for all options.
    /// </summary>
    /// <returns>A new Decision instance with computed scores.</returns>
    public Decision Compute()
    {
        if (!Criteria.Any() || !Options.Any())
            return this with { IsComputed = true };

        // Validate that all options have scores for all criteria
        var missingScores = Options
            .SelectMany(opt => Criteria.Where(crit => !opt.HasScore(crit.Name)).Select(crit => $"{opt.Name} missing {crit.Name}"))
            .ToList();

        if (missingScores.Any())
            throw new InvalidOperationException($"Missing scores: {string.Join(", ", missingScores)}");

        // Calculate score ranges for normalization
        var scoreRanges = Criteria.ToDictionary(
            crit => crit.Name,
            crit => 
            {
                var scores = Options.Select(opt => opt.GetScore(crit.Name)!.Value);
                return (Min: scores.Min(), Max: scores.Max());
            });

        // Normalize scores and compute totals
        var computedOptions = Options.Select(option =>
        {
            var normalizedScores = new Dictionary<string, Score>();
            
            foreach (var criterion in Criteria)
            {
                var originalScore = option.GetScore(criterion.Name)!;
                var range = scoreRanges[criterion.Name];
                var normalizedValue = criterion.NormalizeScore(originalScore.Value, range.Min, range.Max);
                
                normalizedScores[criterion.Name] = originalScore.WithNormalizedValue(normalizedValue);
            }

            var totalScore = ComputeTotalScore(normalizedScores, Criteria, Aggregation);
            var avgConfidence = normalizedScores.Values.Any() 
                ? (ConfidenceLevel)Math.Round(normalizedScores.Values.Average(s => (int)s.Confidence))
                : ConfidenceLevel.Medium;

            return option with 
            { 
                Scores = new ReadOnlyDictionary<string, Score>(normalizedScores),
                TotalScore = totalScore,
                OverallConfidence = avgConfidence
            };
        }).ToList();

        return this with 
        { 
            Options = new ReadOnlyCollection<Option>(computedOptions),
            IsComputed = true 
        };
    }

    /// <summary>
    /// Gets the top-ranked option based on computed scores.
    /// </summary>
    /// <returns>The highest-scoring option, or null if no options exist.</returns>
    public Option? GetRecommendation()
    {
        var computed = IsComputed ? this : Compute();
        return computed.Options.OrderByDescending(opt => opt.TotalScore).FirstOrDefault();
    }

    /// <summary>
    /// Gets all options ranked by score in descending order.
    /// </summary>
    /// <returns>Options sorted by total score (highest first).</returns>
    public IEnumerable<Option> GetRankedOptions()
    {
        var computed = IsComputed ? this : Compute();
        return computed.Options.OrderByDescending(opt => opt.TotalScore);
    }

    /// <summary>
    /// Gets a criterion by name.
    /// </summary>
    /// <param name="name">The name of the criterion.</param>
    /// <returns>The criterion if found, null otherwise.</returns>
    public Criterion? GetCriterion(string name)
    {
        return Criteria.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets an option by name.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <returns>The option if found, null otherwise.</returns>
    public Option? GetOption(string name)
    {
        return Options.FirstOrDefault(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Creates a new Decision with an additional criterion.
    /// </summary>
    /// <param name="criterion">The criterion to add.</param>
    /// <returns>A new Decision instance with the additional criterion.</returns>
    public Decision WithCriterion(Criterion criterion)
    {
        var newCriteria = Criteria.ToList();
        newCriteria.Add(criterion);
        return this with { Criteria = new ReadOnlyCollection<Criterion>(newCriteria), IsComputed = false };
    }

    /// <summary>
    /// Creates a new Decision with an additional option.
    /// </summary>
    /// <param name="option">The option to add.</param>
    /// <returns>A new Decision instance with the additional option.</returns>
    public Decision WithOption(Option option)
    {
        var newOptions = Options.ToList();
        newOptions.Add(option);
        return this with { Options = new ReadOnlyCollection<Option>(newOptions), IsComputed = false };
    }

    /// <summary>
    /// Creates a new Decision with updated aggregation type.
    /// </summary>
    /// <param name="aggregation">The new aggregation type.</param>
    /// <returns>A new Decision instance with updated aggregation.</returns>
    public Decision WithAggregation(AggregationType aggregation)
    {
        return this with { Aggregation = aggregation, IsComputed = false };
    }

    /// <summary>
    /// Creates a new Decision with updated context.
    /// </summary>
    /// <param name="context">The new context.</param>
    /// <returns>A new Decision instance with updated context.</returns>
    public Decision WithContext(string? context)
    {
        return this with { Context = context };
    }

    /// <summary>
    /// Computes the total score for an option based on normalized scores and criteria weights.
    /// </summary>
    private static double ComputeTotalScore(
        IReadOnlyDictionary<string, Score> normalizedScores, 
        IEnumerable<Criterion> criteria, 
        AggregationType aggregation)
    {
        var weightedScores = criteria
            .Where(c => normalizedScores.ContainsKey(c.Name))
            .Select(c => new { Criterion = c, Score = normalizedScores[c.Name] })
            .ToList();

        if (!weightedScores.Any()) return 0.0;

        return aggregation switch
        {
            AggregationType.WeightedAverage => weightedScores.Sum(ws => ws.Criterion.Weight * ws.Score.NormalizedValue),
            AggregationType.GeometricMean => Math.Pow(weightedScores.Aggregate(1.0, (acc, ws) => acc * Math.Pow(ws.Score.NormalizedValue, ws.Criterion.Weight)), 1.0),
            AggregationType.MinScore => weightedScores.Min(ws => ws.Score.NormalizedValue),
            AggregationType.MaxScore => weightedScores.Max(ws => ws.Score.NormalizedValue),
            AggregationType.HarmonicMean => weightedScores.Count / weightedScores.Sum(ws => ws.Criterion.Weight / Math.Max(ws.Score.NormalizedValue, 0.001)),
            _ => weightedScores.Sum(ws => ws.Criterion.Weight * ws.Score.NormalizedValue)
        };
    }

    /// <summary>
    /// Returns a formatted string representation of the decision.
    /// </summary>
    /// <param name="format">Format string: "G" for general, "S" for summary, "F" for full detail.</param>
    /// <param name="formatProvider">Format provider for culture-specific formatting.</param>
    /// <returns>Formatted string representation.</returns>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        formatProvider ??= CultureInfo.CurrentCulture;

        return format?.ToUpperInvariant() switch
        {
            "G" or null => $"{Title}: {Options.Count} options, {Criteria.Count} criteria (Computed: {IsComputed})",
            "S" => GetSummary(),
            "F" => GetFullReport(),
            "T" => Title,
            _ => ToString()
        };
    }

    /// <summary>Returns a string representation of the decision.</summary>
    public override string ToString() => ToString("G", CultureInfo.CurrentCulture);

    /// <summary>
    /// Gets a summary of the decision analysis.
    /// </summary>
    /// <returns>A formatted summary string.</returns>
    public string GetSummary()
    {
        var recommendation = GetRecommendation();
        return $"""
            Decision: {Title}
            Criteria: {Criteria.Count} ({string.Join(", ", Criteria.Select(c => $"{c.Name} ({c.Weight:P0})"))})
            Options: {Options.Count}
            Aggregation: {Aggregation}
            Recommendation: {recommendation?.Name ?? "None"} (Score: {recommendation?.TotalScore:F3 ?? 0})
            Computed: {IsComputed}
            """;
    }

    /// <summary>
    /// Gets a full detailed report of the decision analysis.
    /// </summary>
    /// <returns>A comprehensive formatted report.</returns>
    public string GetFullReport()
    {
        var computed = IsComputed ? this : Compute();
        var report = new System.Text.StringBuilder();
        
        report.AppendLine($"Decision Analysis: {Title}");
        report.AppendLine($"Created: {CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
        if (!string.IsNullOrEmpty(Context))
            report.AppendLine($"Context: {Context}");
        report.AppendLine($"Aggregation Method: {Aggregation}");
        report.AppendLine();

        report.AppendLine("Criteria:");
        foreach (var criterion in Criteria)
        {
            report.AppendLine($"  • {criterion.ToString("G", null)}");
        }
        report.AppendLine();

        report.AppendLine("Options (Ranked):");
        foreach (var (option, index) in computed.GetRankedOptions().Select((opt, i) => (opt, i + 1)))
        {
            report.AppendLine($"{index}. {option.ToString("F", null)}");
            report.AppendLine();
        }

        return report.ToString();
    }

    /// <summary>Returns an enumerator for the options.</summary>
    public IEnumerator<Option> GetEnumerator() => Options.GetEnumerator();

    /// <summary>Returns an enumerator for the options.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}