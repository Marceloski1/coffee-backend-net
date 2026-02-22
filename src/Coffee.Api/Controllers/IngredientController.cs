using Microsoft.AspNetCore.Mvc;
using Coffee.Application.Interfaces;
using Coffee.Application.DTOs;

namespace Coffee.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class IngredientController : ControllerBase
{
    private readonly IIngredientService _ingredientService;

    public IngredientController(IIngredientService ingredientService)
    {
        _ingredientService = ingredientService ?? throw new ArgumentNullException(nameof(ingredientService));
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<IngredientDto>>> GetAll(
        [FromQuery] IngredientQueryDto query,
        CancellationToken ct)
    {
        var result = await _ingredientService.GetAllAsync(query, ct);

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

    [HttpGet("{id}")]
    public async Task<ActionResult<IngredientDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _ingredientService.GetByIdAsync(id, ct);

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

    [HttpPost]
    public async Task<ActionResult<IngredientDto>> Create(
        [FromBody] CreateIngredientDto createDto,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _ingredientService.CreateAsync(createDto, ct);

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

    [HttpPut("{id}")]
    public async Task<ActionResult<IngredientDto>> Update(
        Guid id,
        [FromBody] UpdateIngredientDto updateDto,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _ingredientService.UpdateAsync(id, updateDto, ct);

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

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _ingredientService.DeleteAsync(id, ct);

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
