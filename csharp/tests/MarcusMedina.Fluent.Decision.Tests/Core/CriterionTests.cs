using FluentAssertions;
using MarcusMedina.Fluent.Decision.Core;
using MarcusMedina.Fluent.Decision.Enums;
using Xunit;

namespace MarcusMedina.Fluent.Decision.Tests.Core;

/// <summary>
/// Unit tests for the Criterion class.
/// </summary>
public class CriterionTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateCriterion()
    {
        // Arrange
        const string name = "Performance";
        const double weight = 0.6;
        const NormalizationType normalization = NormalizationType.Linear;
        const string description = "System performance metric";

        // Act
        var criterion = new Criterion(name, weight, normalization, description);

        // Assert
        criterion.Name.Should().Be(name);
        criterion.Weight.Should().Be(weight);
        criterion.Normalization.Should().Be(normalization);
        criterion.Description.Should().Be(description);
    }

    [Fact]
    public void Constructor_WithMinimalParameters_ShouldCreateCriterion()
    {
        // Arrange
        const string name = "Cost";
        const double weight = 0.4;

        // Act
        var criterion = new Criterion(name, weight);

        // Assert
        criterion.Name.Should().Be(name);
        criterion.Weight.Should().Be(weight);
        criterion.Normalization.Should().Be(NormalizationType.Linear);
        criterion.Description.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        const double weight = 0.5;

        // Act
        var act = () => new Criterion(invalidName, weight);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Criterion name cannot be null or empty.*");
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(-1.0)]
    [InlineData(2.0)]
    public void Constructor_WithInvalidWeight_ShouldThrowArgumentException(double invalidWeight)
    {
        // Arrange
        const string name = "TestCriterion";

        // Act
        var act = () => new Criterion(name, invalidWeight);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Weight must be between 0.0 and 1.0.*");
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void Constructor_WithValidWeight_ShouldCreateCriterion(double validWeight)
    {
        // Arrange
        const string name = "TestCriterion";

        // Act
        var criterion = new Criterion(name, validWeight);

        // Assert
        criterion.Weight.Should().Be(validWeight);
    }

    [Fact]
    public void Equality_WithSameCriteria_ShouldBeEqual()
    {
        // Arrange
        var criterion1 = new Criterion("Performance", 0.6, NormalizationType.Linear, "Test description");
        var criterion2 = new Criterion("Performance", 0.6, NormalizationType.Linear, "Test description");

        // Act & Assert
        criterion1.Should().Be(criterion2);
        criterion1.GetHashCode().Should().Be(criterion2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentCriteria_ShouldNotBeEqual()
    {
        // Arrange
        var criterion1 = new Criterion("Performance", 0.6, NormalizationType.Linear);
        var criterion2 = new Criterion("Cost", 0.4, NormalizationType.InverseLinear);

        // Act & Assert
        criterion1.Should().NotBe(criterion2);
    }

    [Fact]
    public void ToString_WithValidCriterion_ShouldReturnFormattedString()
    {
        // Arrange
        var criterion = new Criterion("Performance", 0.6, NormalizationType.Linear);

        // Act
        var result = criterion.ToString();

        // Assert
        result.Should().Contain("Performance");
        result.Should().Contain("60.0");
        result.Should().Contain("Linear");
    }

    [Fact]
    public void ToString_WithCustomFormat_ShouldReturnCustomFormattedString()
    {
        // Arrange
        var criterion = new Criterion("Performance", 0.6, NormalizationType.Linear, "System performance");

        // Act
        var result = criterion.ToString("F", System.Globalization.CultureInfo.InvariantCulture);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Performance");
    }

    [Theory]
    [InlineData(NormalizationType.None)]
    [InlineData(NormalizationType.Linear)]
    [InlineData(NormalizationType.InverseLinear)]
    [InlineData(NormalizationType.Logarithmic)]
    [InlineData(NormalizationType.SquareRoot)]
    public void Constructor_WithAllNormalizationTypes_ShouldCreateCriterion(NormalizationType normalizationType)
    {
        // Arrange
        const string name = "TestCriterion";
        const double weight = 0.5;

        // Act
        var criterion = new Criterion(name, weight, normalizationType);

        // Assert
        criterion.Normalization.Should().Be(normalizationType);
    }

    [Fact]
    public void Description_WhenNull_ShouldAcceptNullValue()
    {
        // Arrange
        const string name = "TestCriterion";
        const double weight = 0.5;

        // Act
        var criterion = new Criterion(name, weight, NormalizationType.Linear, null);

        // Assert
        criterion.Description.Should().BeNull();
    }

    [Fact]
    public void Description_WhenEmpty_ShouldAcceptEmptyValue()
    {
        // Arrange
        const string name = "TestCriterion";
        const double weight = 0.5;
        const string description = "";

        // Act
        var criterion = new Criterion(name, weight, NormalizationType.Linear, description);

        // Assert
        criterion.Description.Should().Be(description);
    }
}