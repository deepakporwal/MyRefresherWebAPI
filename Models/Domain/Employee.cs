namespace MyRefresherWebAPI.Models.Domain;

public sealed class Employee
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Department { get; init; } = string.Empty;
}
