using Blanquita.Application.DTOs;
using FluentValidation;

namespace Blanquita.Application.Validators;

public class CreateCashCollectionValidator : AbstractValidator<CreateCashCollectionDto>
{
    public CreateCashCollectionValidator()
    {
        RuleFor(x => x.CashRegisterName)
            .NotEmpty().WithMessage("La caja registradora es obligatoria.");

        RuleFor(x => x.CashierName)
            .NotEmpty().WithMessage("El nombre de la cajera es obligatorio.");

        RuleFor(x => x.SupervisorName)
            .NotEmpty().WithMessage("El nombre del supervisor es obligatorio.");

        RuleFor(x => x.Thousands).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FiveHundreds).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TwoHundreds).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Hundreds).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Fifties).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Twenties).GreaterThanOrEqualTo(0);

        RuleFor(x => x)
            .Must(HaveSomeMoney)
            .WithMessage("Debe ingresar al menos un billete.");
    }

    private bool HaveSomeMoney(CreateCashCollectionDto dto)
    {
        return dto.Thousands > 0 || 
               dto.FiveHundreds > 0 || 
               dto.TwoHundreds > 0 || 
               dto.Hundreds > 0 || 
               dto.Fifties > 0 || 
               dto.Twenties > 0;
    }
}
