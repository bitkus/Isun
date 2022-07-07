namespace Persistence;

public record  PersistenceRecord<T>(Guid Id, T Data);