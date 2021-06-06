using FluentValidation;
using TrabajosExpres.Models;

namespace TrabajosExpres.Validators
{
    public class CityValidator : AbstractValidator<City>
    {
        public CityValidator()
        {
            RuleFor(city => city.name).NotEmpty().WithState(account => "ComboBoxCity")
                    .MaximumLength(50).WithState(account => "ComboBoxCity")
                    .MinimumLength(5).WithState(account => "ComboBoxCity")
                    .Matches(@"^([A-ZÁÉÍÓÚ]{1}[a-zñáéíóú]+[\\s]*)+\$").WithState(account => "ComboBoxCity");

            RuleFor(city => city.idState).NotEmpty().GreaterThan(0);
        }
    }
}
