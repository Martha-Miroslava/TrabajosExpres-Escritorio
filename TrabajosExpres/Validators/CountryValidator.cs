using FluentValidation;
using TrabajosExpres.Models;

namespace TrabajosExpres.Validators
{
    public class CountryValidator : AbstractValidator<Country>
    {
        public CountryValidator()
        {
            RuleFor(country => country.name).NotEmpty().WithState(country => "TextBoxCountry")
                    .MaximumLength(50).WithState(country => "TextBoxCountry")
                    .MinimumLength(5).WithState(country => "TextBoxCountry")
                    .Matches(@"^([A-ZÁÉÍÓÚ]{1}[a-zñáéíóú]+[\\s]*)+\$").WithState(country => "TextBoxCountry");
        }
    }
}
