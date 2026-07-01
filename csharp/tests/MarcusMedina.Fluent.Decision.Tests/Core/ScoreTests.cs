using FluentAssertions;
using MarcusMedina.Fluent.Decision.Core;
using MarcusMedina.Fluent.Decision.Enums;
using Xunit;

namespace MarcusMedina.Fluent.Decision.Tests.Core;

/// <summary>
/// Unit tests for the Score class.
/// </summary>
public class ScoreTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateScore()
    {
        // Arrange
        const double value = 8.5;
        const ConfidenceLevel confidence = ConfidenceLevel.High;
        const string notes = "Test score notes";
        const double normalizedValue = 0.85;

        // Act
        var score = new Score(value, confidence, notes, normalizedValue);

        // Assert
        score.Value.Should().Be(value);
        score.Confidence.Should().Be(confidence);
        score.Notes.Should().Be(notes);
        score.NormalizedValue.Should().Be(normalizedValue);
    }

    [Fact]
    public void Constructor_WithMinimalParameters_ShouldCreateScore()
    {
        // Arrange
        const double value = 7.0;

        // Act
        var score = new Score(value);

        // Assert
        score.Value.Should().Be(value);
        score.Confidence.Should().Be(ConfidenceLevel.Medium);
        score.Notes.Should().BeNull();
        score.NormalizedValue.Should().Be(0.0);
    }

    [Fact]
    public void WithNormalizedValue_ShouldReturnNewInstanceWithUpdatedValue()
    {
        // Arrange
        var originalScore = new Score(8.5, ConfidenceLevel.High, "Original");
        const double newNormalizedValue = 0.95;

        // Act
        var updatedScore = originalScore.WithNormalizedValue(newNormalizedValue);

        // Assert
        updatedScore.Should().NotBeSameAs(originalScore);
        updatedScore.Value.Should().Be(originalScore.Value);
        updatedScore.Confidence.Should().Be(originalScore.Confidence);
        updatedScore.Notes.Should().Be(originalScore.Notes);
        updatedScore.NormalizedValue.Should().Be(newNormalizedValue);
    }

    [Fact]
    public void CompareTo_WithNull_ShouldReturnPositive()
    {
        // Arrange
        var score = new Score(5.0, normalizedValue: 0.5);

        // Act
        var result = score.CompareTo(null);

        // Assert
        result.Should().BePositive();
    }

    [Fact]
    public void CompareTo_WithHigherNormalizedScore_ShouldReturnNegative()
    {
        // Arrange
        var score1 = new Score(5.0, normalizedValue: 0.5);
        var score2 = new Score(8.0, normalizedValue: 0.8);

        // Act
        var result = score1.CompareTo(score2);

        // Assert
        result.Should().BeNegative();
    }

    [Fact]
    public void CompareTo_WithLowerNormalizedScore_ShouldReturnPositive()
    {
        // Arrange
        var score1 = new Score(8.0, normalizedValue: 0.8);
        var score2 = new Score(5.0, normalizedValue: 0.5);

        // Act
        var result = score1.CompareTo(score2);

        // Assert
        result.Should().BePositive();
    }

    [Fact]
    public void CompareTo_WithEqualNormalizedScore_ShouldReturnZero()
    {
        // Arrange
        var score1 = new Score(8.0, normalizedValue: 0.7);
        var score2 = new Score(7.0, normalizedValue: 0.7);

        // Act
        var result = score1.CompareTo(score2);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var score = new Score(8.75, ConfidenceLevel.High, "Test notes", 0.875);

        // Act
        var result = score.ToString();

        // Assert
        result.Should().Contain("8.75");
        result.Should().Contain("High");
        result.Should().Contain("0.875");
    }

    [Fact]
    public void Equality_WithSameScores_ShouldBeEqual()
    {
        // Arrange
        var score1 = new Score(8.5, ConfidenceLevel.High, "Test", 0.85);
        var score2 = new Score(8.5, ConfidenceLevel.High, "Test", 0.85);

        // Act & Assert
        score1.Should().Be(score2);
        score1.GetHashCode().Should().Be(score2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentScores_ShouldNotBeEqual()
    {
        // Arrange
        var score1 = new Score(8.5, ConfidenceLevel.High);
        var score2 = new Score(7.5, ConfidenceLevel.Medium);

        // Act & Assert
        score1.Should().NotBe(score2);
    }

    [Theory]
    [InlineData(ConfidenceLevel.VeryLow)]
    [InlineData(ConfidenceLevel.Low)]
    [InlineData(ConfidenceLevel.Medium)]
    [InlineData(ConfidenceLevel.High)]
    [InlineData(ConfidenceLevel.VeryHigh)]
    public void Constructor_WithAllConfidenceLevels_ShouldCreateScore(ConfidenceLevel confidence)
    {
        // Arrange
        const double value = 8.0;

        // Act
        var score = new Score(value, confidence);

        // Assert
        score.Confidence.Should().Be(confidence);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(5.5)]
    [InlineData(10.0)]
    [InlineData(-1.0)]
    [InlineData(100.0)]
    public void Constructor_WithVariousValues_ShouldAcceptAllValues(double value)
    {
        // Arrange & Act
        var score = new Score(value);

        // Assert
        score.Value.Should().Be(value);
    }

    [Fact]
    public void Notes_WhenNull_ShouldAcceptNullValue()
    {
        // Arrange
        const double value = 8.0;

        // Act
        var score = new Score(value, ConfidenceLevel.Medium, null);

        // Assert
        score.Notes.Should().BeNull();
    }

    [Fact]
    public void Notes_WhenEmpty_ShouldAcceptEmptyValue()
    {
        // Arrange
        const double value = 8.0;
        const string notes = "";

        // Act
        var score = new Score(value, ConfidenceLevel.Medium, notes);

        // Assert
        score.Notes.Should().Be(notes);
    }

    [Fact]
    public void NormalizedValue_DefaultValue_ShouldBeZero()
    {
        // Arrange & Act
        var score = new Score(8.5);

        // Assert
        score.NormalizedValue.Should().Be(0.0);
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public void WithNormalizedValue_WithVariousValues_ShouldAcceptAllValues(double normalizedValue)
    {
        // Arrange
        var score = new Score(8.0);

        // Act
        var updatedScore = score.WithNormalizedValue(normalizedValue);

        // Assert
        updatedScore.NormalizedValue.Should().Be(normalizedValue);
    }
}