using FluentValidation;
using TrabajosExpres.Models;

namespace TrabajosExpres.Validators
{
    public class StateValidator : AbstractValidator<State>
    {
        public StateValidator()
        {
            RuleFor(state => state.name).NotEmpty().WithState(state => "ComboBoxState")
                   .MaximumLength(50).WithState(state => "ComboBoxState")
                   .MinimumLength(5).WithState(state => "ComboBoxState")
                   .Matches(@"^([A-ZÁÉÍÓÚ]{1}[a-zñáéíóú]+[\\s]*)+\$").WithState(state => "ComboBoxState");

            RuleFor(state => state.idCountry).NotEmpty().GreaterThan(0);


        }
    }
}
