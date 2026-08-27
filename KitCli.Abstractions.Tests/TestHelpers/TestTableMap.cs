using KitCli.Abstractions.Tables;

namespace KitCli.Abstractions.Tests.TestHelpers;

/// <summary>A map covering every member of <see cref="TestAggregate"/>, renaming one of them.</summary>
public class TestTableMap : TableMap<TestAggregate>
{
    public TestTableMap()
    {
        Map(x => x.Name).Name("Label");
        Map(x => x.Description);
    }
}
