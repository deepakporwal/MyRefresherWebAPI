using MyRefresherWebAPI.Models.Domain;

namespace MyRefresherWebAPI.Services;

public sealed class EmployeeService(ILogger<EmployeeService> logger) : IEmployeeService
{
    public Employee GetEmpById(int employeeId)
    {
        logger.LogInformation("Retrieving employee {EmployeeId}.", employeeId);

        // Deliberately throw an exception to demonstrate global exception handling.
        throw new InvalidOperationException(
            $"Employee lookup demonstration failed for employee {employeeId}.");
    }
}
