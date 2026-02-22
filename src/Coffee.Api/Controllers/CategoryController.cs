using Microsoft.AspNetCore.Mvc;
using Coffee.Application.Interfaces;
using Coffee.Application.DTOs;

namespace Coffee.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CategoryDto>>> GetAll(
        [FromQuery] CategoryQueryDto query,
        CancellationToken ct)
    {
        var result = await _categoryService.GetAllAsync(query, ct);

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
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _categoryService.GetByIdAsync(id, ct);

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
    public async Task<ActionResult<CategoryDto>> Create(
        [FromBody] CreateCategoryDto createDto,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _categoryService.CreateAsync(createDto, ct);

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
    public async Task<ActionResult<CategoryDto>> Update(
        Guid id,
        [FromBody] UpdateCategoryDto updateDto,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _categoryService.UpdateAsync(id, updateDto, ct);

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
        var result = await _categoryService.DeleteAsync(id, ct);

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
