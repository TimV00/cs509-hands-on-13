namespace MathTests;

using Xunit;
public class UnitTest1
{
    [Fact]
    public void TestAdd()
    {
        // Arrange
        int a = 2;
        int b = 3;
        int expected = 5;

        // Act
        int result = Math.Math.Add(a, b);

        // Assert
        Assert.Equal(expected, result);
    }
}
