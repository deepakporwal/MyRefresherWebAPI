using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using MyRefresherWebAPI.Models.Domain;
using MyRefresherWebAPI.Services;

namespace MyRefresherWebAPI.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public sealed class EmployeeController(
    IEmployeeService employeeService,
    ILogger<EmployeeController> logger) : ControllerBase
{
    [HttpGet("{employeeId:int}")]
    [ProducesResponseType(typeof(Employee), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<Employee> GetEmpById(int employeeId)
    {
        logger.LogInformation(
            "Getting employee {EmployeeId} for user {User}.",
            employeeId,
            User.Identity?.Name ?? "unknown");

        return Ok(employeeService.GetEmpById(employeeId));
    }
}
