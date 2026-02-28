using Inmemorydatabase;
using Inmemorydatabase.Contracts;
using Moq;
using Xunit;

namespace Inmemorydatabase.Tests;

public class UnitTest1
{
    [Fact]
    public void Select_WithMoqFilter_FiltersAndOrdersCorrectly()
    {
        // Arrange
        var db = new InMemoryDatabase();
        db.CreateTable("users", new[]
        {
            new ColumnDefinition(ColumnType.Integer, "id", IsPrimary: true),
            new ColumnDefinition(ColumnType.String, "name"),
            new ColumnDefinition(ColumnType.Integer, "age")
        });

        db.Insert("users", new Dictionary<string, object> { ["id"] = 1, ["name"] = "Alice", ["age"] = 30 });
        db.Insert("users", new Dictionary<string, object> { ["id"] = 2, ["name"] = "Bob", ["age"] = 25 });
        db.Insert("users", new Dictionary<string, object> { ["id"] = 3, ["name"] = "Charlie", ["age"] = 40 });

        // Moq filter: age > 25
        var filterMock = new Mock<IRowFilter>();
        filterMock
            .Setup(f => f.Evaluate(It.IsAny<IDictionary<string, object>>()))
            .Returns<IDictionary<string, object>>(row => (int)row["age"] > 25);

        // Act
        var results = db.Select("users", filterMock.Object, orderBy: "name", desc: false).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("Alice", results[0]["name"]);
        Assert.Equal("Charlie", results[1]["name"]);

        // Verify filter evaluated for each row
        filterMock.Verify(f => f.Evaluate(It.IsAny<IDictionary<string, object>>()), Times.Exactly(3));
    }
}
