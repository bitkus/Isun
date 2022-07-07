namespace Persistence;

public interface IRepository<T>
{
    T? Get(Guid id);
    void Add(Guid id, T item);
}
