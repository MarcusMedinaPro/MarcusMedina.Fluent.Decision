using MarcusMedina.Fluent.Decision.Core;
using MarcusMedina.Fluent.Decision.Enums;

namespace MarcusMedina.Fluent.Decision.Builders;

/// <summary>
/// Fluent builder for creating Decision instances with a natural, progressive API.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Purpose:</strong> Provides an intuitive, step-by-step interface for building complex decision analyses without requiring deep knowledge of the underlying decision theory.
/// </para>
/// <para>
/// <strong>When to use:</strong> When you want to build decisions progressively, adding criteria and options in a readable, fluent manner.
/// </para>
/// <para>
/// <strong>Example:</strong>
/// <code>
/// var decision = Decision.Create("Choose Cloud Provider")
///     .WithContext("Need to migrate our e-commerce platform")
///     .WithCriterion("Cost", 0.3, NormalizationType.InverseLinear, "Total monthly cost including support")
///     .WithCriterion("Performance", 0.4, NormalizationType.Linear, "Response time and throughput")
///     .WithCriterion("Reliability", 0.3, NormalizationType.Linear, "Uptime SLA and disaster recovery")
///     .WithOption("AWS")
///         .WithScore("Cost", 7.0, ConfidenceLevel.High, "Based on calculator estimate")
///         .WithScore("Performance", 9.0, ConfidenceLevel.High, "Proven in production")
///         .WithScore("Reliability", 9.5, ConfidenceLevel.High, "99.99% SLA")
///     .WithOption("Azure")
///         .WithScore("Cost", 7.5, ConfidenceLevel.Medium, "Similar pricing structure")
///         .WithScore("Performance", 8.5, ConfidenceLevel.Medium, "Good but less experience")
///         .WithScore("Reliability", 9.0, ConfidenceLevel.High, "99.9% SLA")
///     .WithAggregation(AggregationType.WeightedAverage)
///     .Build();
/// </code>
/// </para>
/// </remarks>
public sealed class DecisionBuilder
{
    private readonly string _title;
    private readonly List<Criterion> _criteria = new();
    private readonly List<Option> _options = new();
    private AggregationType _aggregation = AggregationType.WeightedAverage;
    private string? _context;
    private Option? _currentOption;

    /// <summary>
    /// Initializes a new DecisionBuilder with the specified title.
    /// </summary>
    /// <param name="title">The title of the decision.</param>
    /// <exception cref="ArgumentException">Thrown when title is null or empty.</exception>
    public DecisionBuilder(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Decision title cannot be null or empty.", nameof(title));
        
        _title = title;
    }

    /// <summary>
    /// Adds context information to the decision.
    /// </summary>
    /// <param name="context">The decision context or background information.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public DecisionBuilder WithContext(string context)
    {
        _context = context;
        return this;
    }

    /// <summary>
    /// Adds a criterion to the decision.
    /// </summary>
    /// <param name="name">The name of the criterion.</param>
    /// <param name="weight">The weight (importance) of the criterion (0.0 to 1.0).</param>
    /// <param name="normalization">The normalization strategy for scores.</param>
    /// <param name="description">Optional description of the criterion.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when criterion name already exists.</exception>
    public DecisionBuilder WithCriterion(string name, double weight, NormalizationType normalization = NormalizationType.Linear, string? description = null)
    {
        if (_criteria.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Criterion '{name}' already exists.", nameof(name));

        _criteria.Add(new Criterion(name, weight, normalization, description));
        return this;
    }

    /// <summary>
    /// Adds a criterion using a pre-built Criterion object.
    /// </summary>
    /// <param name="criterion">The criterion to add.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when criterion name already exists.</exception>
    public DecisionBuilder WithCriterion(Criterion criterion)
    {
        if (_criteria.Any(c => c.Name.Equals(criterion.Name, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Criterion '{criterion.Name}' already exists.", nameof(criterion));

        _criteria.Add(criterion);
        return this;
    }

    /// <summary>
    /// Starts building a new option with the specified name.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <param name="notes">Optional notes about the option.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when option name already exists.</exception>
    public DecisionBuilder WithOption(string name, string? notes = null)
    {
        // Finalize current option if one exists
        FinalizeCurrentOption();

        if (_options.Any(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Option '{name}' already exists.", nameof(name));

        _currentOption = new Option(name, notes: notes);
        return this;
    }

    /// <summary>
    /// Adds a score to the current option being built.
    /// </summary>
    /// <param name="criterionName">The name of the criterion to score.</param>
    /// <param name="value">The score value.</param>
    /// <param name="confidence">The confidence level in this score.</param>
    /// <param name="notes">Optional notes about the score.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no option is currently being built.</exception>
    /// <exception cref="ArgumentException">Thrown when criterion doesn't exist.</exception>
    public DecisionBuilder WithScore(string criterionName, double value, ConfidenceLevel confidence = ConfidenceLevel.Medium, string? notes = null)
    {
        if (_currentOption == null)
            throw new InvalidOperationException("No option is currently being built. Call WithOption() first.");

        if (!_criteria.Any(c => c.Name.Equals(criterionName, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Criterion '{criterionName}' does not exist. Add it with WithCriterion() first.", nameof(criterionName));

        _currentOption = _currentOption.WithScore(criterionName, value, confidence, notes);
        return this;
    }

    /// <summary>
    /// Adds a score using a pre-built Score object to the current option.
    /// </summary>
    /// <param name="criterionName">The name of the criterion to score.</param>
    /// <param name="score">The score object.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no option is currently being built.</exception>
    /// <exception cref="ArgumentException">Thrown when criterion doesn't exist.</exception>
    public DecisionBuilder WithScore(string criterionName, Score score)
    {
        if (_currentOption == null)
            throw new InvalidOperationException("No option is currently being built. Call WithOption() first.");

        if (!_criteria.Any(c => c.Name.Equals(criterionName, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Criterion '{criterionName}' does not exist. Add it with WithCriterion() first.", nameof(criterionName));

        _currentOption = _currentOption.WithScore(criterionName, score);
        return this;
    }

    /// <summary>
    /// Sets the aggregation method for combining criterion scores.
    /// </summary>
    /// <param name="aggregation">The aggregation method to use.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public DecisionBuilder WithAggregation(AggregationType aggregation)
    {
        _aggregation = aggregation;
        return this;
    }

    /// <summary>
    /// Validates the current builder state and builds the Decision.
    /// </summary>
    /// <returns>A new Decision instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the builder state is invalid.</exception>
    public Core.Decision Build()
    {
        // Finalize current option if one exists
        FinalizeCurrentOption();

        // Validate builder state
        ValidateBuilderState();

        return new Core.Decision(
            title: _title,
            criteria: _criteria,
            options: _options,
            aggregation: _aggregation,
            context: _context
        );
    }

    /// <summary>
    /// Validates the current builder state and builds a computed Decision.
    /// </summary>
    /// <returns>A new Decision instance with computed scores.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the builder state is invalid.</exception>
    public Core.Decision BuildAndCompute()
    {
        return Build().Compute();
    }

    /// <summary>
    /// Finalizes the current option being built and adds it to the options list.
    /// </summary>
    private void FinalizeCurrentOption()
    {
        if (_currentOption != null)
        {
            _options.Add(_currentOption);
            _currentOption = null;
        }
    }

    /// <summary>
    /// Validates that the builder is in a valid state for building a Decision.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
    private void ValidateBuilderState()
    {
        if (!_criteria.Any())
            throw new InvalidOperationException("At least one criterion must be defined.");

        if (!_options.Any())
            throw new InvalidOperationException("At least one option must be defined.");

        // Check that weights sum to approximately 1.0
        var totalWeight = _criteria.Sum(c => c.Weight);
        if (Math.Abs(totalWeight - 1.0) > 0.001)
            throw new InvalidOperationException($"Criterion weights must sum to 1.0 (current sum: {totalWeight:F3}).");

        // Check that all options have scores for all criteria
        var missingScores = new List<string>();
        foreach (var option in _options)
        {
            foreach (var criterion in _criteria)
            {
                if (!option.HasScore(criterion.Name))
                {
                    missingScores.Add($"Option '{option.Name}' missing score for criterion '{criterion.Name}'");
                }
            }
        }

        if (missingScores.Any())
            throw new InvalidOperationException($"Missing scores: {string.Join(", ", missingScores)}");
    }

    /// <summary>
    /// Gets the current criteria that have been added to the builder.
    /// </summary>
    /// <returns>A read-only collection of criteria.</returns>
    public IReadOnlyCollection<Criterion> GetCriteria() => _criteria.AsReadOnly();

    /// <summary>
    /// Gets the current options that have been added to the builder.
    /// </summary>
    /// <returns>A read-only collection of options.</returns>
    public IReadOnlyCollection<Option> GetOptions()
    {
        var options = new List<Option>(_options);
        if (_currentOption != null)
            options.Add(_currentOption);
        return options.AsReadOnly();
    }

    /// <summary>
    /// Gets the name of the option currently being built.
    /// </summary>
    /// <returns>The current option name, or null if no option is being built.</returns>
    public string? GetCurrentOptionName() => _currentOption?.Name;

    /// <summary>
    /// Clears all criteria from the builder.
    /// </summary>
    /// <returns>The builder instance for fluent chaining.</returns>
    public DecisionBuilder ClearCriteria()
    {
        _criteria.Clear();
        return this;
    }

    /// <summary>
    /// Clears all options from the builder.
    /// </summary>
    /// <returns>The builder instance for fluent chaining.</returns>
    public DecisionBuilder ClearOptions()
    {
        _options.Clear();
        _currentOption = null;
        return this;
    }

    /// <summary>
    /// Resets the builder to its initial state.
    /// </summary>
    /// <returns>The builder instance for fluent chaining.</returns>
    public DecisionBuilder Reset()
    {
        _criteria.Clear();
        _options.Clear();
        _currentOption = null;
        _aggregation = AggregationType.WeightedAverage;
        _context = null;
        return this;
    }
}