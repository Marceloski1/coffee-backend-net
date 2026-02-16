using System.ComponentModel.DataAnnotations;

namespace Coffee.Application.DTOs;

public class CoffeeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateCoffeeDto
{
    [Required(ErrorMessage = "El nombre del café es requerido")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Name { get; set; } = string.Empty;
}

public class UpdateCoffeeDto
{
    [Required(ErrorMessage = "El nombre del café es requerido")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Name { get; set; } = string.Empty;
}

public class CoffeeQueryDto
{
    [StringLength(50, ErrorMessage = "La búsqueda no puede exceder 50 caracteres")]
    public string? Search { get; set; }
    
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "La página debe ser mayor que 0")]
    public int Page { get; set; } = 1;
    
    [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100")]
    public int PageSize { get; set; } = 10;
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}