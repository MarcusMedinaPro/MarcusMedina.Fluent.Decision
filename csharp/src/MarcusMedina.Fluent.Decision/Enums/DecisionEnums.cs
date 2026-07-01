namespace MarcusMedina.Fluent.Decision.Enums;

/// <summary>
/// Defines the normalization strategy for criterion scores.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Purpose:</strong> Controls how raw scores are converted to normalized values for consistent comparison across different criteria with different scales.
/// </para>
/// <para>
/// <strong>When to use:</strong> When you have criteria with different measurement scales (e.g., cost in dollars vs. satisfaction on 1-10 scale).
/// </para>
/// <para>
/// <strong>Example:</strong>
/// <code>
/// var costCriterion = new Criterion("Cost", 0.4, NormalizationType.InverseLinear);
/// var satisfactionCriterion = new Criterion("Satisfaction", 0.6, NormalizationType.Linear);
/// </code>
/// </para>
/// </remarks>
public enum NormalizationType
{
    /// <summary>No normalization - use raw scores as-is.</summary>
    None = 0,
    
    /// <summary>Linear normalization where higher values are better (0-1 scale).</summary>
    Linear = 1,
    
    /// <summary>Inverse linear normalization where lower values are better (0-1 scale).</summary>
    InverseLinear = 2,
    
    /// <summary>Logarithmic normalization for exponential differences.</summary>
    Logarithmic = 3,
    
    /// <summary>Square root normalization for moderate scaling.</summary>
    SquareRoot = 4
}

/// <summary>
/// Defines the aggregation method for combining criterion scores into a final decision score.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Purpose:</strong> Determines how individual criterion scores are mathematically combined to produce the overall option score.
/// </para>
/// <para>
/// <strong>When to use:</strong> When you need different mathematical approaches to decision aggregation based on your decision-making philosophy.
/// </para>
/// <para>
/// <strong>Example:</strong>
/// <code>
/// var decision = Decision.Create("Choose Technology Stack")
///     .WithAggregation(AggregationType.WeightedAverage)
///     .AddCriterion("Performance", 0.4)
///     .Build();
/// </code>
/// </para>
/// </remarks>
public enum AggregationType
{
    /// <summary>Weighted average - multiply each score by its weight and sum.</summary>
    WeightedAverage = 0,
    
    /// <summary>Weighted geometric mean - good for avoiding compensation between criteria.</summary>
    GeometricMean = 1,
    
    /// <summary>Minimum score approach - conservative, focuses on worst performance.</summary>
    MinScore = 2,
    
    /// <summary>Maximum score approach - optimistic, focuses on best performance.</summary>
    MaxScore = 3,
    
    /// <summary>Harmonic mean - penalizes options with very low scores in any criterion.</summary>
    HarmonicMean = 4
}

/// <summary>
/// Defines the confidence level in a decision or option evaluation.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Purpose:</strong> Captures the subjective confidence level in decision inputs and outputs for transparency and future review.
/// </para>
/// <para>
/// <strong>When to use:</strong> When you want to record uncertainty levels for later analysis or when decisions involve significant uncertainty.
/// </para>
/// <para>
/// <strong>Example:</strong>
/// <code>
/// var option = Option.Create("Cloud Migration")
///     .WithScore("Cost", 7.5, ConfidenceLevel.Medium)
///     .WithScore("Security", 9.0, ConfidenceLevel.High)
///     .Build();
/// </code>
/// </para>
/// </remarks>
public enum ConfidenceLevel
{
    /// <summary>Very low confidence - significant uncertainty in assessment.</summary>
    VeryLow = 1,
    
    /// <summary>Low confidence - considerable uncertainty.</summary>
    Low = 2,
    
    /// <summary>Medium confidence - moderate uncertainty.</summary>
    Medium = 3,
    
    /// <summary>High confidence - little uncertainty.</summary>
    High = 4,
    
    /// <summary>Very high confidence - minimal uncertainty.</summary>
    VeryHigh = 5
}