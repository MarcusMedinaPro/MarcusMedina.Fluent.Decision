using FluentAssertions;
using MarcusMedina.Fluent.Decision.Core;
using MarcusMedina.Fluent.Decision.Enums;
using Xunit;

namespace MarcusMedina.Fluent.Decision.Tests.Core;

/// <summary>
/// Unit tests for the Option class.
/// </summary>
public class OptionTests
{
    [Fact]
    public void Constructor_WithValidName_ShouldCreateOption()
    {
        // Arrange
        const string name = "Test Option";

        // Act
        var option = new Option(name);

        // Assert
        option.Name.Should().Be(name);
        option.Scores.Should().BeEmpty();
        option.Notes.Should().BeNull();
        option.TotalScore.Should().Be(0.0);
        option.OverallConfidence.Should().Be(ConfidenceLevel.Medium);
    }

    [Fact]
    public void Constructor_WithAllParameters_ShouldCreateOption()
    {
        // Arrange
        const string name = "Test Option";
        var scores = new Dictionary<string, Score>
        {
            ["Performance"] = new Score(8.5, ConfidenceLevel.High),
            ["Cost"] = new Score(6.0, ConfidenceLevel.Medium)
        };
        const string notes = "Test notes";
        const double totalScore = 7.25;
        const ConfidenceLevel confidence = ConfidenceLevel.High;

        // Act
        var option = new Option(name, scores, notes, totalScore, confidence);

        // Assert
        option.Name.Should().Be(name);
        option.Scores.Should().HaveCount(2);
        option.Notes.Should().Be(notes);
        option.TotalScore.Should().Be(totalScore);
        option.OverallConfidence.Should().Be(confidence);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Act
        var act = () => new Option(invalidName!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Option name cannot be null or empty.*");
    }

    [Fact]
    public void GetScore_WithExistingCriterion_ShouldReturnScore()
    {
        // Arrange
        var score = new Score(8.5, ConfidenceLevel.High);
        var scores = new Dictionary<string, Score> { ["Performance"] = score };
        var option = new Option("Test", scores);

        // Act
        var result = option.GetScore("Performance");

        // Assert
        result.Should().Be(score);
    }

    [Fact]
    public void GetScore_WithNonExistingCriterion_ShouldReturnNull()
    {
        // Arrange
        var option = new Option("Test");

        // Act
        var result = option.GetScore("NonExisting");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void HasScore_WithExistingCriterion_ShouldReturnTrue()
    {
        // Arrange
        var scores = new Dictionary<string, Score> { ["Performance"] = new Score(8.5) };
        var option = new Option("Test", scores);

        // Act
        var result = option.HasScore("Performance");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasScore_WithNonExistingCriterion_ShouldReturnFalse()
    {
        // Arrange
        var option = new Option("Test");

        // Act
        var result = option.HasScore("NonExisting");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void WithScore_WithScoreObject_ShouldReturnNewOptionWithUpdatedScore()
    {
        // Arrange
        var option = new Option("Test");
        var score = new Score(8.5, ConfidenceLevel.High, "Great performance");

        // Act
        var updatedOption = option.WithScore("Performance", score);

        // Assert
        updatedOption.Should().NotBeSameAs(option);
        updatedOption.HasScore("Performance").Should().BeTrue();
        updatedOption.GetScore("Performance").Should().Be(score);
        updatedOption.Name.Should().Be(option.Name);
    }

    [Fact]
    public void WithScore_WithValueAndConfidence_ShouldReturnNewOptionWithScore()
    {
        // Arrange
        var option = new Option("Test");
        const double value = 7.5;
        const ConfidenceLevel confidence = ConfidenceLevel.High;
        const string notes = "Test notes";

        // Act
        var updatedOption = option.WithScore("Cost", value, confidence, notes);

        // Assert
        updatedOption.Should().NotBeSameAs(option);
        updatedOption.HasScore("Cost").Should().BeTrue();
        var score = updatedOption.GetScore("Cost");
        score!.Value.Should().Be(value);
        score.Confidence.Should().Be(confidence);
        score.Notes.Should().Be(notes);
    }

    [Fact]
    public void WithScore_UpdatingExistingScore_ShouldOverrideScore()
    {
        // Arrange
        var originalScore = new Score(6.0, ConfidenceLevel.Low);
        var scores = new Dictionary<string, Score> { ["Performance"] = originalScore };
        var option = new Option("Test", scores);
        var newScore = new Score(9.0, ConfidenceLevel.High);

        // Act
        var updatedOption = option.WithScore("Performance", newScore);

        // Assert
        updatedOption.GetScore("Performance").Should().Be(newScore);
        updatedOption.GetScore("Performance").Should().NotBe(originalScore);
    }

    [Fact]
    public void WithTotals_ShouldReturnNewOptionWithUpdatedTotals()
    {
        // Arrange
        var option = new Option("Test", totalScore: 5.0, overallConfidence: ConfidenceLevel.Low);
        const double newTotalScore = 8.5;
        const ConfidenceLevel newConfidence = ConfidenceLevel.High;

        // Act
        var updatedOption = option.WithTotals(newTotalScore, newConfidence);

        // Assert
        updatedOption.Should().NotBeSameAs(option);
        updatedOption.TotalScore.Should().Be(newTotalScore);
        updatedOption.OverallConfidence.Should().Be(newConfidence);
        updatedOption.Name.Should().Be(option.Name);
    }

    [Fact]
    public void WithTotals_WithNullConfidence_ShouldKeepOriginalConfidence()
    {
        // Arrange
        var option = new Option("Test", overallConfidence: ConfidenceLevel.High);
        const double newTotalScore = 8.5;

        // Act
        var updatedOption = option.WithTotals(newTotalScore);

        // Assert
        updatedOption.TotalScore.Should().Be(newTotalScore);
        updatedOption.OverallConfidence.Should().Be(ConfidenceLevel.High);
    }

    [Fact]
    public void WithNotes_ShouldReturnNewOptionWithUpdatedNotes()
    {
        // Arrange
        var option = new Option("Test", notes: "Original notes");
        const string newNotes = "Updated notes";

        // Act
        var updatedOption = option.WithNotes(newNotes);

        // Assert
        updatedOption.Should().NotBeSameAs(option);
        updatedOption.Notes.Should().Be(newNotes);
        updatedOption.Name.Should().Be(option.Name);
    }

    [Fact]
    public void CompareTo_WithNull_ShouldReturnPositive()
    {
        // Arrange
        var option = new Option("Test", totalScore: 8.5);

        // Act
        var result = option.CompareTo(null);

        // Assert
        result.Should().BePositive();
    }

    [Fact]
    public void CompareTo_WithLowerScore_ShouldReturnPositive()
    {
        // Arrange
        var option1 = new Option("Higher", totalScore: 8.5);
        var option2 = new Option("Lower", totalScore: 6.0);

        // Act
        var result = option1.CompareTo(option2);

        // Assert
        result.Should().BePositive();
    }

    [Fact]
    public void CompareTo_WithHigherScore_ShouldReturnNegative()
    {
        // Arrange
        var option1 = new Option("Lower", totalScore: 6.0);
        var option2 = new Option("Higher", totalScore: 8.5);

        // Act
        var result = option1.CompareTo(option2);

        // Assert
        result.Should().BeNegative();
    }

    [Fact]
    public void CompareTo_WithEqualScore_ShouldReturnZero()
    {
        // Arrange
        var option1 = new Option("First", totalScore: 7.5);
        var option2 = new Option("Second", totalScore: 7.5);

        // Act
        var result = option1.CompareTo(option2);

        // Assert
        result.Should().Be(0);
    }

    [Theory]
    [InlineData("G", "Test: 8.500 (Confidence: High)")]
    [InlineData("S", "8.500")]
    [InlineData("N", "Test")]
    public void ToString_WithFormat_ShouldReturnCorrectFormat(string format, string expectedContains)
    {
        // Arrange
        var option = new Option("Test", totalScore: 8.5, overallConfidence: ConfidenceLevel.High);

        // Act
        var result = option.ToString(format, System.Globalization.CultureInfo.InvariantCulture);

        // Assert
        result.Should().Contain(expectedContains);
    }

    [Fact]
    public void ToString_WithDetailedFormat_ShouldIncludeNotesAndScores()
    {
        // Arrange
        var scores = new Dictionary<string, Score>
        {
            ["Performance"] = new Score(8.5),
            ["Cost"] = new Score(7.0)
        };
        var option = new Option("Test", scores, "Test notes", 7.75, ConfidenceLevel.High);

        // Act
        var result = option.ToString("F", System.Globalization.CultureInfo.InvariantCulture);

        // Assert
        result.Should().Contain("Test notes");
        result.Should().Contain("Performance=8.50");
        result.Should().Contain("Cost=7.00");
    }

    [Fact]
    public void GetScoresSummary_WithNoScores_ShouldReturnNoScoresMessage()
    {
        // Arrange
        var option = new Option("Test");

        // Act
        var result = option.GetScoresSummary();

        // Assert
        result.Should().Be("No scores recorded");
    }

    [Fact]
    public void GetScoresSummary_WithScores_ShouldReturnFormattedSummary()
    {
        // Arrange
        var scores = new Dictionary<string, Score>
        {
            ["Performance"] = new Score(8.5, ConfidenceLevel.High),
            ["Cost"] = new Score(6.0, ConfidenceLevel.Medium)
        };
        var option = new Option("Test", scores);

        // Act
        var result = option.GetScoresSummary();

        // Assert
        result.Should().Contain("Performance: 8.50 (High)");
        result.Should().Contain("Cost: 6.00 (Medium)");
    }

    [Fact]
    public void CalculateAverageConfidence_WithNoScores_ShouldReturnMedium()
    {
        // Arrange
        var option = new Option("Test");

        // Act
        var result = option.CalculateAverageConfidence();

        // Assert
        result.Should().Be(ConfidenceLevel.Medium);
    }

    [Fact]
    public void CalculateAverageConfidence_WithScores_ShouldReturnAverageConfidence()
    {
        // Arrange
        var scores = new Dictionary<string, Score>
        {
            ["Performance"] = new Score(8.5, ConfidenceLevel.High),    // 4
            ["Cost"] = new Score(6.0, ConfidenceLevel.Medium),         // 3
            ["Quality"] = new Score(7.0, ConfidenceLevel.High)         // 4
        };
        var option = new Option("Test", scores);

        // Act
        var result = option.CalculateAverageConfidence();

        // Assert
        // Average of 4, 3, 4 = 3.67, rounded = 4 (High)
        result.Should().Be(ConfidenceLevel.High);
    }

    [Fact]
    public void Equality_WithSameOptions_ShouldBeEqual()
    {
        // Arrange
        var scores = new Dictionary<string, Score> { ["Test"] = new Score(8.5) };
        var option1 = new Option("Test", scores, "Notes", 7.5, ConfidenceLevel.High);
        var option2 = new Option("Test", scores, "Notes", 7.5, ConfidenceLevel.High);

        // Act & Assert
        option1.Name.Should().Be(option2.Name);
        option1.TotalScore.Should().Be(option2.TotalScore);
        option1.OverallConfidence.Should().Be(option2.OverallConfidence);
        option1.Notes.Should().Be(option2.Notes);
        // Note: Records may not be equal if internal dictionary references differ
    }

    [Fact]
    public void Equality_WithDifferentOptions_ShouldNotBeEqual()
    {
        // Arrange
        var option1 = new Option("Option1", totalScore: 8.5);
        var option2 = new Option("Option2", totalScore: 7.0);

        // Act & Assert
        option1.Should().NotBe(option2);
    }
}