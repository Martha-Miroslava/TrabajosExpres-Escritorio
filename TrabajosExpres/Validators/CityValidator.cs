using FluentValidation;
using TrabajosExpres.Models;

namespace TrabajosExpres.Validators
{
    public class CityValidator : AbstractValidator<City>
    {
        public CityValidator()
        {
            RuleFor(city => city.name).NotEmpty().WithState(city => "TextBoxCity")
                    .MaximumLength(50).WithState(city => "TextBoxCity")
                    .MinimumLength(5).WithState(city => "TextBoxCity")
                    .Matches(@"^([A-ZÁÉÍÓÚ]{1}[a-zñáéíóú]+[\\s]*)+\$").WithState(city => "TextBoxCity");

            RuleFor(city => city.idState).NotEmpty().GreaterThan(0);
        }
    }
}
