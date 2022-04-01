using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Netcorext.EntityFramework.UserIdentityPattern.Extensions;

public static class EntityEntryExtension
{
    public static void UpdateProperty<TEntity, TProperty>(this EntityEntry<TEntity> entry, Expression<Func<TEntity, TProperty>> propertyExpression, TProperty value)
        where TEntity : class
    {
        if (entry.Property(propertyExpression).OriginalValue == null && value == null) return;
        if (entry.Property(propertyExpression).OriginalValue != null && entry.Property(propertyExpression).OriginalValue!.Equals(value)) return;
        entry.Property(propertyExpression).CurrentValue = value;
        entry.Property(propertyExpression).IsModified = true;
    }
}