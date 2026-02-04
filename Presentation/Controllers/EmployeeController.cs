using System.ComponentModel.DataAnnotations;
using EmployeeContactApi.Application.Commands.Dto;
using EmployeeContactApi.Application.Commands.Handlers;
using EmployeeContactApi.Application.Queries.Dto;
using EmployeeContactApi.Application.Queries.Handlers;
using EmployeeContactApi.Application.Services;
using EmployeeContactApi.Presentation.Dto;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeContactApi.Presentation.Controllers;

[ApiController]
[Route("api/employee")]
[Produces("application/json")]
public class EmployeeController : ControllerBase
{
    private readonly ICreateEmployeeCommandHandler _commandHandler;
    private readonly IEmployeeQueryHandler _queryHandler;
    private readonly EmployeeParserService _parserService;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(
        ICreateEmployeeCommandHandler commandHandler,
        IEmployeeQueryHandler queryHandler,
        EmployeeParserService parserService,
        ILogger<EmployeeController> logger)
    {
        _commandHandler = commandHandler;
        _queryHandler = queryHandler;
        _parserService = parserService;
        _logger = logger;
    }

    /// <summary>
    /// Get all employees with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(EmployeePageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmployeePageResponse>> GetAllEmployees(
        [FromQuery][Range(0, int.MaxValue)] int page = 0,
        [FromQuery][Range(1, 100)] int pageSize = 10)
    {
        _logger.LogInformation(">> [EmployeeController.GetAllEmployees] Request received with page={Page}, pageSize={PageSize}", page, pageSize);
        var result = await _queryHandler.FindAllAsync(page, pageSize);
        _logger.LogInformation("<< [EmployeeController.GetAllEmployees] Completed");
        return Ok(result);
    }

    /// <summary>
    /// Get employees by name
    /// </summary>
    [HttpGet("{name}")]
    [ProducesResponseType(typeof(List<EmployeeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<EmployeeResponse>>> GetEmployeeByName([FromRoute] string name)
    {
        _logger.LogInformation(">> [EmployeeController.GetEmployeeByName] Request received with name={Name}", name);
        var result = await _queryHandler.FindByNameAsync(name);
        _logger.LogInformation("<< [EmployeeController.GetEmployeeByName] Completed");
        return Ok(result);
    }

    /// <summary>
    /// Create employees from file upload (CSV or JSON)
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CreateEmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateEmployeeResponse>> CreateEmployeesFromFile(IFormFile file)
    {
        if (file == null)
        {
            return BadRequest(new ErrorResponse(400, "Bad Request", "File cannot be null"));
        }

        _logger.LogInformation(">> [EmployeeController.CreateEmployeesFromFile] Request received with file={FileName}", file.FileName);

        using var reader = new StreamReader(file.OpenReadStream());
        var content = await reader.ReadToEndAsync();
        var commands = _parserService.Parse(content, file.ContentType, file.FileName);
        var count = await _commandHandler.HandleBatchAsync(commands);

        _logger.LogInformation("<< [EmployeeController.CreateEmployeesFromFile] Completed with count={Count}", count);
        return StatusCode(StatusCodes.Status201Created, new CreateEmployeeResponse(count));
    }

    /// <summary>
    /// Create employees from JSON body
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(CreateEmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateEmployeeResponse>> CreateEmployeesFromJson([FromBody] List<CreateEmployeeRequest> requests)
    {
        _logger.LogInformation(">> [EmployeeController.CreateEmployeesFromJson] Request received with {Count} employees", requests.Count);

        var commands = requests.Select(r => new CreateEmployeeCommand(
            name: r.Name,
            email: r.Email,
            tel: r.Tel,
            joined: r.Joined
        )).ToList();

        var count = await _commandHandler.HandleBatchAsync(commands);

        _logger.LogInformation("<< [EmployeeController.CreateEmployeesFromJson] Completed with count={Count}", count);
        return StatusCode(StatusCodes.Status201Created, new CreateEmployeeResponse(count));
    }

    /// <summary>
    /// Create employees from CSV body
    /// </summary>
    [HttpPost]
    [Consumes("text/csv")]
    [ProducesResponseType(typeof(CreateEmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateEmployeeResponse>> CreateEmployeesFromCsvBody([FromBody] string csvContent)
    {
        _logger.LogInformation(">> [EmployeeController.CreateEmployeesFromCsvBody] Request received");

        var commands = _parserService.Parse(csvContent, "text/csv", null);
        var count = await _commandHandler.HandleBatchAsync(commands);

        _logger.LogInformation("<< [EmployeeController.CreateEmployeesFromCsvBody] Completed with count={Count}", count);
        return StatusCode(StatusCodes.Status201Created, new CreateEmployeeResponse(count));
    }
}
