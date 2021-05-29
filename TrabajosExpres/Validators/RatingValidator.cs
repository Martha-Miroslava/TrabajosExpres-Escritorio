using FluentValidation;
using TrabajosExpres.Models;

namespace TrabajosExpres.Validators
{
    public class RatingValidator : AbstractValidator<Rating>
    {
        public RatingValidator()
        {
            RuleFor(rating => rating.comment).NotEmpty().WithState(rating => "TextBoxComment")
                    .MaximumLength(150).WithState(rating => "TextBoxComment")
                    .MinimumLength(5).WithState(rating => "TextBoxComment")
                    .Matches("[a-zA-ZÁÉÍÓÚáéíóú+]").WithState(rating => "TextBoxComment");

            RuleFor(rating => rating.rating).NotEmpty()
                .GreaterThan(0).LessThan(6);

            RuleFor(rating => rating.idRequest).NotEmpty().GreaterThan(0);
        }
    }
}
