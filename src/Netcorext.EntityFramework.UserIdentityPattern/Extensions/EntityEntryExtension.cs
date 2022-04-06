using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Netcorext.EntityFramework.UserIdentityPattern.Extensions;

public static class EntityEntryExtension
{
    public static EntityEntry<TEntity> UpdateProperty<TEntity, TProperty>(this EntityEntry<TEntity> entry, Expression<Func<TEntity, TProperty>> propertyExpression, TProperty value, bool ignoreEmptyValue = true, Action<TEntity>? onChange = null)
        where TEntity : class
    {
        switch (ignoreEmptyValue)
        {
            case true when value == null:
            case true when typeof(TProperty) == typeof(string) && string.IsNullOrWhiteSpace("" + value):
                return entry;
        }

        if (entry.Property(propertyExpression).OriginalValue == null && value == null) return entry;
        if (entry.Property(propertyExpression).OriginalValue != null && entry.Property(propertyExpression).OriginalValue!.Equals(value)) return entry;

        entry.Property(propertyExpression).CurrentValue = value;
        entry.Property(propertyExpression).IsModified = true;

        onChange?.Invoke(entry.Entity);

        return entry;
    }
}