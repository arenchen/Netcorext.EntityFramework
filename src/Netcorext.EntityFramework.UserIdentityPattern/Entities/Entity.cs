namespace Netcorext.EntityFramework.UserIdentityPattern.Entities;

public abstract class Entity
{
    public virtual long Id { get; set; }
    public virtual DateTimeOffset CreationDate { get; set; }
    public virtual long CreatorId { get; set; }
    public virtual DateTimeOffset ModificationDate { get; set; }
    public virtual long ModifierId { get; set; }
    public virtual long Version { get; set; }
}