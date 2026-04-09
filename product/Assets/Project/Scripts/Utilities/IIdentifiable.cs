// All database items must implement this so they can be stored by ID
public interface IIdentifiable
{
    string Id { get; }
}