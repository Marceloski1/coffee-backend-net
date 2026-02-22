using FluentValidation;
using Coffee.Application.DTOs;

namespace Coffee.Application.Validators;

public class CreateIngredientDtoValidator : AbstractValidator<CreateIngredientDto>
{
    public CreateIngredientDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del ingrediente es requerido")
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres")
            .MaximumLength(50).WithMessage("El nombre no puede exceder 50 caracteres")
            .Matches(@"^[a-zA-Z0-9\s\-']+$").WithMessage("El nombre solo puede contener letras, números, espacios, guiones y apóstrofes");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("La descripción no puede exceder 200 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

public class UpdateIngredientDtoValidator : AbstractValidator<UpdateIngredientDto>
{
    public UpdateIngredientDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del ingrediente es requerido")
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres")
            .MaximumLength(50).WithMessage("El nombre no puede exceder 50 caracteres")
            .Matches(@"^[a-zA-Z0-9\s\-']+$").WithMessage("El nombre solo puede contener letras, números, espacios, guiones y apóstrofes");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("La descripción no puede exceder 200 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

public class IngredientQueryDtoValidator : AbstractValidator<IngredientQueryDto>
{
    public IngredientQueryDtoValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("La página debe ser mayor que 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("El tamaño de página debe estar entre 1 y 100");

        RuleFor(x => x.Search)
            .MaximumLength(50).WithMessage("La búsqueda no puede exceder 50 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Search));
    }
}
