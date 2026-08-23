using System.Linq.Expressions;
using System.Reflection;

namespace KitCli.Abstractions.Tables;

/// <summary>
/// Base class for declaring how members of <typeparamref name="TAggregate"/> map to table columns.
/// Derived classes call <see cref="Map{TMember}"/> to register a column for each member they want displayed.
/// </summary>
/// <typeparam name="TAggregate">The aggregate type whose members are being mapped to table columns.</typeparam>
public class TableMap<TAggregate>
{
    /// <summary>
    /// The registered column mappings, keyed by the member they were declared for.
    /// </summary>
    public readonly Dictionary<MemberInfo, TableColumnMap> ColumnMaps = new();

    /// <summary>
    /// Registers a table column for the member referenced by <paramref name="mapExpression"/>.
    /// </summary>
    /// <typeparam name="TMember">The type of the mapped member.</typeparam>
    /// <param name="mapExpression">A member-access expression identifying the property to map, e.g. <c>x =&gt; x.Name</c>.</param>
    /// <returns>The <see cref="TableColumnMap"/> created for the member, which can be further customized (e.g. via <see cref="TableColumnMap.Name"/>).</returns>
    /// <exception cref="Exception">Thrown when <paramref name="mapExpression"/> is not a simple member-access expression.</exception>
    protected TableColumnMap Map<TMember>(Expression<Func<TAggregate, TMember>> mapExpression)
    {
        if (mapExpression.Body is not MemberExpression memberExpression)
        {
            throw new Exception($"Expression {mapExpression} is not a member expression");
        }

        var member = memberExpression.Member;
        var memberMap = new TableColumnMap(member.Name);
        ColumnMaps[member] = memberMap;

        return memberMap;
    }
}