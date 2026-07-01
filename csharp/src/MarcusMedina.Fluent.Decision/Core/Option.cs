using MarcusMedina.Fluent.Decision.Enums;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MarcusMedina.Fluent.Decision.Core;

/// <summary>
/// Represents a decision option with scores for multiple criteria.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Purpose:</strong> Encapsulates a single choice option with its performance scores across all decision criteria and computed total score.
/// </para>
/// <para>
/// <strong>When to use:</strong> When modeling the alternatives you're choosing between, such as different technologies, vendors, or strategies.
/// </para>
/// <para>
/// <strong>Example:</strong>
/// <code>
/// var cloudOption = Option.Create("AWS Cloud")
///     .WithScore("Cost", 7.5)
///     .WithScore("Performance", 9.0)
///     .WithScore("Security", 8.5)
///     .WithNotes("Good scalability, established market leader")
///     .Build();
/// </code>
/// </para>
/// </remarks>
public sealed record Option : IComparable<Option>, IFormattable
{
    /// <summary>Gets the name of this option.</summary>
    public string Name { get; init; }
    
    /// <summary>Gets the scores for each criterion.</summary>
    public ReadOnlyDictionary<string, Score> Scores { get; init; }
    
    /// <summary>Gets optional notes about this option.</summary>
    public string? Notes { get; init; }
    
    /// <summary>Gets the computed total score.</summary>
    public double TotalScore { get; init; }
    
    /// <summary>Gets the overall confidence level for this option.</summary>
    public ConfidenceLevel OverallConfidence { get; init; }

    /// <summary>
    /// Initializes a new instance of the Option record.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <param name="scores">Dictionary of criterion scores.</param>
    /// <param name="notes">Optional notes about the option.</param>
    /// <param name="totalScore">The computed total score.</param>
    /// <param name="overallConfidence">The overall confidence level.</param>
    /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
    public Option(string name, 
                  IReadOnlyDictionary<string, Score>? scores = null, 
                  string? notes = null, 
                  double totalScore = 0.0,
                  ConfidenceLevel overallConfidence = ConfidenceLevel.Medium)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Option name cannot be null or empty.", nameof(name));

        Name = name;
        Scores = new ReadOnlyDictionary<string, Score>(scores?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, Score>());
        Notes = notes;
        TotalScore = totalScore;
        OverallConfidence = overallConfidence;
    }

    /// <summary>
    /// Gets the score for a specific criterion.
    /// </summary>
    /// <param name="criterionName">The name of the criterion.</param>
    /// <returns>The score if it exists, null otherwise.</returns>
    public Score? GetScore(string criterionName)
    {
        return Scores.TryGetValue(criterionName, out var score) ? score : null;
    }

    /// <summary>
    /// Checks if this option has a score for the specified criterion.
    /// </summary>
    /// <param name="criterionName">The name of the criterion.</param>
    /// <returns>True if the option has a score for the criterion.</returns>
    public bool HasScore(string criterionName)
    {
        return Scores.ContainsKey(criterionName);
    }

    /// <summary>
    /// Creates a new Option with an additional or updated score.
    /// </summary>
    /// <param name="criterionName">The name of the criterion.</param>
    /// <param name="score">The score to add or update.</param>
    /// <returns>A new Option instance with the updated score.</returns>
    public Option WithScore(string criterionName, Score score)
    {
        var newScores = new Dictionary<string, Score>(Scores) { [criterionName] = score };
        return this with { Scores = new ReadOnlyDictionary<string, Score>(newScores) };
    }

    /// <summary>
    /// Creates a new Option with an additional or updated score.
    /// </summary>
    /// <param name="criterionName">The name of the criterion.</param>
    /// <param name="value">The score value.</param>
    /// <param name="confidence">The confidence level.</param>
    /// <param name="notes">Optional notes about the score.</param>
    /// <returns>A new Option instance with the updated score.</returns>
    public Option WithScore(string criterionName, double value, ConfidenceLevel confidence = ConfidenceLevel.Medium, string? notes = null)
    {
        return WithScore(criterionName, new Score(value, confidence, notes));
    }

    /// <summary>
    /// Creates a new Option with updated total score and overall confidence.
    /// </summary>
    /// <param name="totalScore">The new total score.</param>
    /// <param name="overallConfidence">The new overall confidence.</param>
    /// <returns>A new Option instance with updated totals.</returns>
    public Option WithTotals(double totalScore, ConfidenceLevel? overallConfidence = null)
    {
        return this with 
        { 
            TotalScore = totalScore, 
            OverallConfidence = overallConfidence ?? OverallConfidence 
        };
    }

    /// <summary>
    /// Creates a new Option with updated notes.
    /// </summary>
    /// <param name="notes">The new notes.</param>
    /// <returns>A new Option instance with updated notes.</returns>
    public Option WithNotes(string? notes)
    {
        return this with { Notes = notes };
    }

    /// <summary>
    /// Compares this option to another based on total score.
    /// </summary>
    /// <param name="other">The other option to compare to.</param>
    /// <returns>Comparison result (higher scores are better).</returns>
    public int CompareTo(Option? other)
    {
        if (other is null) return 1;
        return TotalScore.CompareTo(other.TotalScore);
    }

    /// <summary>
    /// Returns a formatted string representation of the option.
    /// </summary>
    /// <param name="format">Format string: "G" for general, "S" for score only, "N" for name only.</param>
    /// <param name="formatProvider">Format provider for culture-specific formatting.</param>
    /// <returns>Formatted string representation.</returns>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        formatProvider ??= CultureInfo.CurrentCulture;

        return format?.ToUpperInvariant() switch
        {
            "G" or null => $"{Name}: {TotalScore:F3} (Confidence: {OverallConfidence})",
            "S" => TotalScore.ToString("F3", formatProvider),
            "N" => Name,
            "D" => $"{Name}: {TotalScore:F3}\n{Notes ?? "No notes"}",
            "F" => $"{Name}: {TotalScore:F3} (Confidence: {OverallConfidence})\nScores: {string.Join(", ", Scores.Select(kvp => $"{kvp.Key}={kvp.Value.Value:F2}"))}\n{Notes ?? "No notes"}",
            _ => ToString()
        };
    }

    /// <summary>Returns a string representation of the option.</summary>
    public override string ToString() => ToString("G", CultureInfo.CurrentCulture);

    /// <summary>
    /// Gets a summary of scores for display purposes.
    /// </summary>
    /// <returns>A formatted string showing all criterion scores.</returns>
    public string GetScoresSummary()
    {
        if (!Scores.Any()) return "No scores recorded";
        
        return string.Join(", ", Scores.Select(kvp => 
            $"{kvp.Key}: {kvp.Value.Value:F2} ({kvp.Value.Confidence})"));
    }

    /// <summary>
    /// Calculates the average confidence level across all scores.
    /// </summary>
    /// <returns>The average confidence level.</returns>
    public ConfidenceLevel CalculateAverageConfidence()
    {
        if (!Scores.Any()) return ConfidenceLevel.Medium;
        
        var avgConfidence = Scores.Values.Average(s => (int)s.Confidence);
        return (ConfidenceLevel)Math.Round(avgConfidence);
    }
}