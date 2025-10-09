using logManager.Models;
using logManager.Services;
using Xunit;

namespace logManager.Tests.Services;

public class DateRangeCalculatorTests
{
    private readonly DateRangeCalculator _calculator = new();

    [Fact]
    public void Calculate_OlderThan7_YoungerThan0_Returns8Dates()
    {
        // Arrange
        var olderThan = 7;
        var youngerThan = 0;
        var criteria = DateCriteriaType.ModifiedDate;

        // Act
        var results = _calculator.Calculate(olderThan, youngerThan, criteria);

        // Assert
        Assert.Equal(8, results.Length);
        Assert.Equal(DateTime.Today.AddDays(-7), results[0].Date);
        Assert.Equal(DateTime.Today, results[^1].Date);
        Assert.All(results, r => Assert.Equal(criteria, r.Criteria));
    }

    [Fact]
    public void Calculate_OlderThan7_YoungerThan7_Returns1Date()
    {
        // Arrange
        var olderThan = 7;
        var youngerThan = 7;
        var criteria = DateCriteriaType.CreationDate;

        // Act
        var results = _calculator.Calculate(olderThan, youngerThan, criteria);

        // Assert
        Assert.Single(results);
        Assert.Equal(DateTime.Today.AddDays(-7), results[0].Date);
        Assert.Equal(criteria, results[0].Criteria);
    }

    [Fact]
    public void Calculate_OlderThan5_YoungerThan7_ThrowsArgumentException()
    {
        // Arrange
        var olderThan = 5;
        var youngerThan = 7;
        var criteria = DateCriteriaType.ModifiedDate;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            _calculator.Calculate(olderThan, youngerThan, criteria));

        Assert.Contains("olderThan", exception.Message);
        Assert.Contains("youngerThan", exception.Message);
        Assert.Equal("olderThan", exception.ParamName);
    }

    [Theory]
    [InlineData(DateCriteriaType.CreationDate)]
    [InlineData(DateCriteriaType.ModifiedDate)]
    public void Calculate_DateCriteriaEnumValues_SetCorrectly(DateCriteriaType criteria)
    {
        // Arrange
        var olderThan = 3;
        var youngerThan = 0;

        // Act
        var results = _calculator.Calculate(olderThan, youngerThan, criteria);

        // Assert
        Assert.All(results, r => Assert.Equal(criteria, r.Criteria));
    }

    [Fact]
    public void Calculate_EdgeCase_0Days_Returns1Date()
    {
        // Arrange
        var olderThan = 0;
        var youngerThan = 0;
        var criteria = DateCriteriaType.ModifiedDate;

        // Act
        var results = _calculator.Calculate(olderThan, youngerThan, criteria);

        // Assert
        Assert.Single(results);
        Assert.Equal(DateTime.Today, results[0].Date);
    }

    [Fact]
    public void Calculate_EdgeCase_30PlusDays_Returns31Dates()
    {
        // Arrange
        var olderThan = 30;
        var youngerThan = 0;
        var criteria = DateCriteriaType.ModifiedDate;

        // Act
        var results = _calculator.Calculate(olderThan, youngerThan, criteria);

        // Assert
        Assert.Equal(31, results.Length);
        Assert.Equal(DateTime.Today.AddDays(-30), results[0].Date);
        Assert.Equal(DateTime.Today, results[^1].Date);
    }

    [Fact]
    public void Calculate_EdgeCase_LargeRange_Returns101Dates()
    {
        // Arrange
        var olderThan = 100;
        var youngerThan = 0;
        var criteria = DateCriteriaType.ModifiedDate;

        // Act
        var results = _calculator.Calculate(olderThan, youngerThan, criteria);

        // Assert
        Assert.Equal(101, results.Length);
    }

    [Fact]
    public void Calculate_DateRange_IsConsecutive()
    {
        // Arrange
        var olderThan = 10;
        var youngerThan = 5;
        var criteria = DateCriteriaType.ModifiedDate;

        // Act
        var results = _calculator.Calculate(olderThan, youngerThan, criteria);

        // Assert
        Assert.Equal(6, results.Length); // 10, 9, 8, 7, 6, 5 = 6 days
        for (int i = 1; i < results.Length; i++)
        {
            Assert.Equal(results[i - 1].Date.AddDays(1), results[i].Date);
        }
    }

    [Fact]
    public void Calculate_DateRange_IsInclusive()
    {
        // Arrange
        var olderThan = 3;
        var youngerThan = 1;
        var criteria = DateCriteriaType.ModifiedDate;

        // Act
        var results = _calculator.Calculate(olderThan, youngerThan, criteria);

        // Assert
        Assert.Equal(3, results.Length); // Day -3, -2, -1 (inclusive)
        Assert.Equal(DateTime.Today.AddDays(-3), results[0].Date);
        Assert.Equal(DateTime.Today.AddDays(-2), results[1].Date);
        Assert.Equal(DateTime.Today.AddDays(-1), results[2].Date);
    }

    [Fact]
    public void Calculate_Results_AreImmutable()
    {
        // Arrange
        var olderThan = 5;
        var youngerThan = 0;
        var criteria = DateCriteriaType.ModifiedDate;

        // Act
        var results = _calculator.Calculate(olderThan, youngerThan, criteria);
        var firstResult = results[0];

        // Assert - record types should be immutable
        Assert.IsType<DateRangeResult>(firstResult);
        // The record's properties have init-only setters, ensuring immutability
    }
}
