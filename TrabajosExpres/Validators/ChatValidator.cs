using FluentValidation;
using TrabajosExpres.Models;

namespace TrabajosExpres.Validators
{
    public class ChatValidator : AbstractValidator<Chat>
    {
        public ChatValidator()
        {
            RuleFor(chat => chat.idService).NotEmpty().GreaterThan(0);

            RuleFor(chat => chat.idMemberATE).NotEmpty().GreaterThan(0);

            RuleFor(chat => chat.idRequest).NotEmpty().GreaterThan(0);
        }
    }
}
