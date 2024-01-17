using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Netcorext.EntityFramework.UserIdentityPattern.Entities.Mapping;

public abstract class EntityMap<TEntity> where TEntity : Entity
{
    public EntityMap(ModelBuilder modelBuilder, bool isTable = true)
    {
        Builder = modelBuilder.Entity<TEntity>();

        if (isTable) Builder.ToTable(typeof(TEntity).Name);

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

        Builder.Property(t => t.RequestId)
               .HasColumnName(nameof(Entity.RequestId))
               .HasMaxLength(200);

        Builder.Property(t => t.Version)
               .HasColumnName(nameof(Entity.Version));
    }

    public EntityTypeBuilder<TEntity> Builder { get; }
}
