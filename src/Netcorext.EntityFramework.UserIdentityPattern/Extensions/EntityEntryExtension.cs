using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Netcorext.EntityFramework.UserIdentityPattern.Extensions;

public static class EntityEntryExtension
{
    public static EntityEntry<TEntity> UpdateProperty<TEntity, TProperty>(this EntityEntry<TEntity> entry, Expression<Func<TEntity, TProperty>> propertyExpression, TProperty value, bool ignoreNull = true, Action<TEntity>? onChange = null)
        where TEntity : class
    {
        switch (ignoreNull)
        {
            case true when value == null:
                return entry;
        }

        var propertyName = propertyExpression.Body switch
                           {
                               MemberExpression e => e.Member.Name,
                               UnaryExpression e => ((MemberExpression)e.Operand).Member.Name,
                               _ => throw new NotSupportedException(propertyExpression.Body.GetType().Name)
                           };

        if (entry.Property(propertyName).OriginalValue == null && value == null) return entry;
        if (entry.Property(propertyName).OriginalValue != null && entry.Property(propertyName).OriginalValue!.Equals(value)) return entry;

        entry.Property(propertyName).CurrentValue = value;
        entry.Property(propertyName).IsModified = true;

        onChange?.Invoke(entry.Entity);

        return entry;
    }
}
