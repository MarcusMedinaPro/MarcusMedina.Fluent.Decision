using MarcusMedina.Fluent.Decision.Enums;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MarcusMedina.Fluent.Decision.Core;

/// <summary>
/// Represents a decision criterion with weight and normalization strategy.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Purpose:</strong> Defines a factor used to evaluate options with configurable importance weighting and score normalization.
/// </para>
/// <para>
/// <strong>When to use:</strong> When defining the factors that matter in your decision, such as cost, quality, time, risk, etc.
/// </para>
/// <para>
/// <strong>Example:</strong>
/// <code>
/// var costCriterion = new Criterion("Cost", 0.3, NormalizationType.InverseLinear);
/// var qualityCriterion = new Criterion("Quality", 0.7, NormalizationType.Linear);
/// </code>
/// </para>
/// </remarks>
public sealed record Criterion : IEquatable<Criterion>, IFormattable
{
    /// <summary>Gets the name of the criterion.</summary>
    public string Name { get; init; }
    
    /// <summary>Gets the weight of this criterion (0.0 to 1.0).</summary>
    public double Weight { get; init; }
    
    /// <summary>Gets the normalization type for this criterion.</summary>
    public NormalizationType Normalization { get; init; }
    
    /// <summary>Gets optional description of the criterion.</summary>
    public string? Description { get; init; }

    /// <summary>
    /// Initializes a new instance of the Criterion record.
    /// </summary>
    /// <param name="name">The name of the criterion.</param>
    /// <param name="weight">The weight (0.0 to 1.0).</param>
    /// <param name="normalization">The normalization strategy.</param>
    /// <param name="description">Optional description.</param>
    /// <exception cref="ArgumentException">Thrown when name is null or empty, or weight is outside valid range.</exception>
    public Criterion(string name, double weight, NormalizationType normalization = NormalizationType.Linear, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Criterion name cannot be null or empty.", nameof(name));
        if (weight < 0.0 || weight > 1.0)
            throw new ArgumentException("Weight must be between 0.0 and 1.0.", nameof(weight));

        Name = name;
        Weight = weight;
        Normalization = normalization;
        Description = description;
    }

    /// <summary>
    /// Normalizes a raw score according to this criterion's normalization type.
    /// </summary>
    /// <param name="rawScore">The raw score to normalize.</param>
    /// <param name="minValue">The minimum value in the score range.</param>
    /// <param name="maxValue">The maximum value in the score range.</param>
    /// <returns>The normalized score (typically 0.0 to 1.0).</returns>
    public double NormalizeScore(double rawScore, double minValue, double maxValue)
    {
        if (maxValue <= minValue) return 0.0;

        return Normalization switch
        {
            NormalizationType.None => rawScore,
            NormalizationType.Linear => (rawScore - minValue) / (maxValue - minValue),
            NormalizationType.InverseLinear => (maxValue - rawScore) / (maxValue - minValue),
            NormalizationType.Logarithmic => Math.Log(1 + rawScore - minValue) / Math.Log(1 + maxValue - minValue),
            NormalizationType.SquareRoot => Math.Sqrt((rawScore - minValue) / (maxValue - minValue)),
            _ => rawScore
        };
    }

    /// <summary>
    /// Returns a string representation of the criterion.
    /// </summary>
    /// <param name="format">Format string: "G" for general, "W" for weight only, "N" for name only.</param>
    /// <param name="formatProvider">Format provider for culture-specific formatting.</param>
    /// <returns>Formatted string representation.</returns>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        formatProvider ??= CultureInfo.CurrentCulture;

        return format?.ToUpperInvariant() switch
        {
            "G" or null => $"{Name} (Weight: {Weight:P1}, Normalization: {Normalization})",
            "W" => Weight.ToString("P1", formatProvider),
            "N" => Name,
            "D" => Description ?? Name,
            _ => ToString()
        };
    }

    /// <summary>Returns a string representation of the criterion.</summary>
    public override string ToString() => ToString("G", CultureInfo.CurrentCulture);
}

/// <summary>
/// Represents a scored value for a specific criterion with optional confidence level.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Purpose:</strong> Encapsulates a criterion score with metadata about confidence and normalization.
/// </para>
/// <para>
/// <strong>When to use:</strong> When recording how well an option performs on a specific criterion.
/// </para>
/// <para>
/// <strong>Example:</strong>
/// <code>
/// var costScore = new Score(1500.0, ConfidenceLevel.High, "Based on vendor quote");
/// var qualityScore = new Score(8.5, ConfidenceLevel.Medium, "Estimated from similar projects");
/// </code>
/// </para>
/// </remarks>
public sealed record Score : IComparable<Score>
{
    /// <summary>Gets the raw score value.</summary>
    public double Value { get; init; }
    
    /// <summary>Gets the confidence level in this score.</summary>
    public ConfidenceLevel Confidence { get; init; }
    
    /// <summary>Gets optional notes about this score.</summary>
    public string? Notes { get; init; }
    
    /// <summary>Gets the normalized score value (set by the decision framework).</summary>
    public double NormalizedValue { get; init; }

    /// <summary>
    /// Initializes a new instance of the Score record.
    /// </summary>
    /// <param name="value">The raw score value.</param>
    /// <param name="confidence">The confidence level.</param>
    /// <param name="notes">Optional notes about the score.</param>
    /// <param name="normalizedValue">The normalized value (typically set by framework).</param>
    public Score(double value, ConfidenceLevel confidence = ConfidenceLevel.Medium, string? notes = null, double normalizedValue = 0.0)
    {
        Value = value;
        Confidence = confidence;
        Notes = notes;
        NormalizedValue = normalizedValue;
    }

    /// <summary>
    /// Creates a new Score with updated normalized value.
    /// </summary>
    /// <param name="normalizedValue">The new normalized value.</param>
    /// <returns>A new Score instance with the updated normalized value.</returns>
    public Score WithNormalizedValue(double normalizedValue) => this with { NormalizedValue = normalizedValue };

    /// <summary>
    /// Compares this score to another based on normalized value.
    /// </summary>
    /// <param name="other">The other score to compare to.</param>
    /// <returns>Comparison result.</returns>
    public int CompareTo(Score? other)
    {
        if (other is null) return 1;
        return NormalizedValue.CompareTo(other.NormalizedValue);
    }

    /// <summary>Returns a string representation of the score.</summary>
    public override string ToString() => $"{Value:F2} (Confidence: {Confidence}, Normalized: {NormalizedValue:F3})";
}