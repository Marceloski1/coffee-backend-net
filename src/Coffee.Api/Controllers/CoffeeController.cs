using Microsoft.AspNetCore.Mvc;
using Coffee.Application.Interfaces;
using Coffee.Application.DTOs;

namespace Coffee.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class CoffeeController : ControllerBase
{
    private readonly ICoffeeService _coffeeService;

    public CoffeeController(ICoffeeService coffeeService)
    {
        _coffeeService = coffeeService ?? throw new ArgumentNullException(nameof(coffeeService));
    }

    /// <summary>
    /// Obtiene todos los cafés con paginación, búsqueda y filtros
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<CoffeeDto>>> GetAll(
        [FromQuery] CoffeeQueryDto query,
        CancellationToken ct)
    {
        var result = await _coffeeService.GetAllAsync(query, ct);

        if (result.IsFailure)
        {
            return result.ErrorCode switch
            {
                "VALIDATION_ERROR" => BadRequest(new { error = result.Error, code = result.ErrorCode }),
                _ => StatusCode(500, new { error = result.Error, code = result.ErrorCode })
            };
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Obtiene un café por su ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CoffeeDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _coffeeService.GetByIdAsync(id, ct);

        if (result.IsFailure)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error, code = result.ErrorCode }),
                "INVALID_ID" => BadRequest(new { error = result.Error, code = result.ErrorCode }),
                _ => StatusCode(500, new { error = result.Error, code = result.ErrorCode })
            };
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Crea un nuevo café
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CoffeeDto>> Create(
        [FromBody] CreateCoffeeDto createDto,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _coffeeService.CreateAsync(createDto, ct);

        if (result.IsFailure)
        {
            return result.ErrorCode switch
            {
                "VALIDATION_ERROR" => BadRequest(new { error = result.Error, code = result.ErrorCode }),
                "DUPLICATE_NAME" => Conflict(new { error = result.Error, code = result.ErrorCode }),
                _ => StatusCode(500, new { error = result.Error, code = result.ErrorCode })
            };
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            result.Value);
    }

    /// <summary>
    /// Actualiza un café existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CoffeeDto>> Update(
        Guid id,
        [FromBody] UpdateCoffeeDto updateDto,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _coffeeService.UpdateAsync(id, updateDto, ct);

        if (result.IsFailure)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error, code = result.ErrorCode }),
                "INVALID_ID" => BadRequest(new { error = result.Error, code = result.ErrorCode }),
                "VALIDATION_ERROR" => BadRequest(new { error = result.Error, code = result.ErrorCode }),
                "DUPLICATE_NAME" => Conflict(new { error = result.Error, code = result.ErrorCode }),
                _ => StatusCode(500, new { error = result.Error, code = result.ErrorCode })
            };
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Elimina un café
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _coffeeService.DeleteAsync(id, ct);

        if (result.IsFailure)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error, code = result.ErrorCode }),
                "INVALID_ID" => BadRequest(new { error = result.Error, code = result.ErrorCode }),
                _ => StatusCode(500, new { error = result.Error, code = result.ErrorCode })
            };
        }

        return NoContent();
    }
}
