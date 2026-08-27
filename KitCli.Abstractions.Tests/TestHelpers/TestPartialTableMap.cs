using KitCli.Abstractions.Tables;

namespace KitCli.Abstractions.Tests.TestHelpers;

/// <summary>A map leaving <see cref="TestAggregate.Description"/> unmapped.</summary>
public class TestPartialTableMap : TableMap<TestAggregate>
{
    public TestPartialTableMap()
    {
        Map(x => x.Name);
    }
}
