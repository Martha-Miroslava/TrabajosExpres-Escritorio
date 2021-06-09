using FluentValidation;
using TrabajosExpres.Models;

namespace TrabajosExpres.Validators
{
    public class ServiceValidator : AbstractValidator<Service>
    {
        public ServiceValidator()
        {
            RuleFor(service => service.name).NotEmpty().WithState(service => "TextBoxName")
                    .MaximumLength(150).WithState(service => "TextBoxName")
                    .MinimumLength(5).WithState(service => "TextBoxName")
                    .Matches("^[a-zA-ZÁÉÍÓÚáéíóú0-9 ]+$").WithState(service => "TextBoxName");

            RuleFor(service => service.description).NotEmpty().WithState(service => "TextBoxDescription")
                    .MaximumLength(300).WithState(service => "TextBoxDescription")
                    .MinimumLength(5).WithState(service => "TextBoxDescription")
                    .Matches("[a-zA-ZÁÉÍÓÚáéíóú+]").WithState(service => "TextBoxDescription");

            RuleFor(service => service.slogan).NotEmpty().WithState(service => "TextBoxSlogan")
                    .MaximumLength(50).WithState(service => "TextBoxSlogan")
                    .MinimumLength(5).WithState(service => "TextBoxSlogan")
                    .Matches("^[a-zA-ZÁÉÍÓÚáéíóú0-9 ]+$").WithState(service => "TextBoxSlogan");
            
            RuleFor(service => service.typeService).NotEmpty().WithState(service => "TextBoxTypeService")
                    .MaximumLength(150).WithState(service => "TextBoxTypeService")
                    .MinimumLength(5).WithState(service => "TextBoxTypeService")
                    .Matches("^[a-zA-ZÁÉÍÓÚáéíóú ]+$").WithState(service => "TextBoxTypeService");

            RuleFor(service => service.workingHours).NotEmpty().WithState(service => "TextBoxWorkingHours")
                    .MaximumLength(150).WithState(service => "TextBoxWorkingHours")
                    .MinimumLength(1).WithState(service => "TextBoxWorkingHours")
                    .Matches("^[a-zA-ZÁÉÍÓÚáéíóú.:,0-9 ]+$").WithState(service => "TextBoxWorkingHours");

            RuleFor(service => service.minimalCost).NotEmpty().WithState(service => "TextBoxMinimalCost")
                .GreaterThan(-1).WithState(service => "TextBoxMinimalCost");
            
            RuleFor(service => service.maximumCost).NotEmpty().WithState(service => "TextBoxMaximumCost")
                .GreaterThan(0).WithState(service => "TextBoxMaximumCost");

            RuleFor(service => service.idMemberATE).NotEmpty().GreaterThan(0);

            RuleFor(service => service.idCity).NotEmpty().GreaterThan(0);
        }
    }
}
