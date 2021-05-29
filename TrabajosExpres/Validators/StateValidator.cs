using FluentValidation;
using TrabajosExpres.Models;

namespace TrabajosExpres.Validators
{
    public class StateValidator : AbstractValidator<State>
    {
        public StateValidator()
        {
            RuleFor(state => state.name).NotEmpty().WithState(state => "TextBoxState")
                   .MaximumLength(50).WithState(state => "TextBoxState")
                   .MinimumLength(5).WithState(state => "TextBoxState")
                   .Matches(@"^([A-ZÁÉÍÓÚ]{1}[a-zñáéíóú]+[\\s]*)+\$").WithState(state => "TextBoxState");

            RuleFor(state => state.idCountry).NotEmpty().GreaterThan(0);


        }
    }
}
