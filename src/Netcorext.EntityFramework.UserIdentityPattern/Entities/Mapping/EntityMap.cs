using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Netcorext.EntityFramework.UserIdentityPattern.Entities.Mapping;

public abstract class EntityMap<TEntity> where TEntity : Entity
{
    public EntityMap(ModelBuilder modelBuilder)
    {
        Builder = modelBuilder.Entity<TEntity>();

        Builder.ToTable(typeof(TEntity).Name);

        Builder.HasKey(t => t.Id);

        // Properties
        Builder.Property(t => t.Id)
               .HasColumnName(nameof(Entity.Id))
               .ValueGeneratedNever();

        Builder.Property(t => t.CreationDate)
               .HasColumnName(nameof(Entity.CreationDate));

        Builder.Property(t => t.CreatorId)
               .HasColumnName(nameof(Entity.CreatorId));

        Builder.Property(t => t.ModificationDate)
               .HasColumnName(nameof(Entity.ModificationDate));

        Builder.Property(t => t.ModifierId)
               .HasColumnName(nameof(Entity.ModifierId));

        Builder.Property(t => t.Version)
               .HasColumnName(nameof(Entity.Version))
               .IsConcurrencyToken();
    }

    public EntityTypeBuilder<TEntity> Builder { get; }
}