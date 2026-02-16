using System.Linq.Expressions;
using System.Reflection;

namespace KitCli.Abstractions.Tables;

public class TableMap<TAggregate>
{
    public readonly Dictionary<MemberInfo, TableColumnMap> ColumnMaps = new();
    
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