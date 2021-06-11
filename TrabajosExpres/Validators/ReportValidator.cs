using FluentValidation;
using TrabajosExpres.Models;

namespace TrabajosExpres.Validators
{
    public class ReportValidator : AbstractValidator<Report>
    {
        public ReportValidator()
        {
            RuleFor(report => report.reason).NotEmpty().WithState(report => "TextBoxReason")
                    .MaximumLength(100).WithState(report => "TextBoxReason")
                    .MinimumLength(3).WithState(report => "TextBoxReason")
                    .Matches("^[a-zA-ZÁÉÍÓÚáéíóú0-9.,;: ]+$").WithState(report => "TextBoxReason");

            RuleFor(report => report.idMemberATE).NotEmpty().GreaterThan(0);

            RuleFor(report => report.idService).NotEmpty().GreaterThan(0);
        }
    }
}
