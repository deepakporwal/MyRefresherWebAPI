using MyRefresherWebAPI.Models.Domain;

namespace MyRefresherWebAPI.Services;

public interface IEmployeeService
{
    Employee GetEmpById(int employeeId);
}
