using FluentValidation;
using TrabajosExpres.Models;

namespace TrabajosExpres.Validators
{
    public class MessageValidator : AbstractValidator<Message>
    {
        public MessageValidator()
        {
            RuleFor(message => message.message).NotEmpty().WithState(message => "TextBoxMessage")
                    .MinimumLength(1).WithState(message => "TextBoxMessage")
                    .Matches("[a-zA-Z+]").WithState(message => "TextBoxMessage");

            RuleFor(message => message.idChat).NotEmpty().GreaterThan(0);

            RuleFor(message => message.dateTime).NotEmpty();

            RuleFor(message => message.memberATEType).NotEmpty().GreaterThan(0);
        }
    }
}
