namespace Contracts;

public record City(string Name)
{
    public virtual bool Equals(City? other) => Name.ToLowerInvariant() == other?.Name.ToLowerInvariant();
    public override int GetHashCode() => Name.ToLowerInvariant().GetHashCode();
}