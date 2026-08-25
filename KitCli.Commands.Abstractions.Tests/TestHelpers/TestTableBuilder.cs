using KitCli.Abstractions.Tables;

namespace KitCli.Commands.Abstractions.Tests.TestHelpers;

/// <summary>A table builder with no configuration beyond what a test gives it.</summary>
public class TestTableBuilder : TableBuilder<TestAggregate, TestAggregate>;
