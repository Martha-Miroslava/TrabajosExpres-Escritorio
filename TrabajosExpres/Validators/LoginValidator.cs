using FluentValidation;
using TrabajosExpres.Models;

namespace TrabajosExpres.Validators
{
    public class LoginValidator : AbstractValidator<Login>
    {
        public LoginValidator()
        {
            RuleFor(login => login.username).NotEmpty().WithState(login => "TextBoxUsername")
                    .MaximumLength(20).WithState(login => "TextBoxUsername")
                    .MaximumLength(8).WithState(login => "TextBoxUsername")
                    .Matches(@"^([A-Z0-9]{1}[a-z0-9]+[\\s0-9]*)+\$").WithState(user => "TextBoxUsername");

            RuleFor(login => login.password).NotEmpty().WithState(login => "PasswordBoxPassword")
                    .MaximumLength(15).WithState(login => "PasswordBoxPassword")
                    .MinimumLength(8).WithState(login => "PasswordBoxPassword")
                    .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[\$@\$!%*?&#])[A-Za-z\\d\$@\$!%*?&#]{8,15}").WithState(account => "PasswordBoxPassword");
        }
    }
}
