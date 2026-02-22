using System.ComponentModel.DataAnnotations;

namespace Coffee.Application.DTOs;

public class IngredientDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateIngredientDto
{
    [Required(ErrorMessage = "El nombre del ingrediente es requerido")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdateIngredientDto
{
    [Required(ErrorMessage = "El nombre del ingrediente es requerido")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public class IngredientQueryDto
{
    [StringLength(50, ErrorMessage = "La búsqueda no puede exceder 50 caracteres")]
    public string? Search { get; set; }
    
    public bool? IsActive { get; set; }
    
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "La página debe ser mayor que 0")]
    public int Page { get; set; } = 1;
    
    [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100")]
    public int PageSize { get; set; } = 10;
}
