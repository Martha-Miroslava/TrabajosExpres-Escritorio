using FluentValidation;
using TrabajosExpres.Models;

namespace TrabajosExpres.Validators
{
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(request => request.address).NotEmpty().WithState(request => "TextBoxAddress")
                    .MaximumLength(254).WithState(request => "TextBoxAddress")
                    .MinimumLength(10).WithState(request => "TextBoxAddress")
                    .Matches("[a-zA-ZÁÉÍÓÚáéíóú+]").WithState(request => "TextBoxAddress");

            RuleFor(request => request.date).NotEmpty().WithState(request => "TextBoxDate");

            RuleFor(request => request.time).NotEmpty().WithState(request => "TextBoxTime");

            RuleFor(request => request.trouble).NotEmpty().WithState(request => "TextBoxTrouble")
                    .MaximumLength(200).WithState(request => "TextBoxTrouble")
                    .MinimumLength(8).WithState(request => "TextBoxTrouble")
                    .Matches("[a-zA-ZÁÉÍÓÚáéíóú+]").WithState(request => "TextBoxTrouble");
            
            RuleFor(request => request.idMemberATE).NotEmpty().GreaterThan(0);
            
            RuleFor(request => request.idService).NotEmpty().GreaterThan(0);
        }
    }
}
