using FluentValidation;
using System.Text.RegularExpressions;
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
                    .Matches("^[A-Za-z0-9]+$").WithState(user => "TextBoxUsername");

            RuleFor(login => login.password).NotEmpty().WithState(login => "PasswordBoxPassword")
                    .MaximumLength(15).WithState(login => "PasswordBoxPassword")
                    .MinimumLength(8).WithState(login => "PasswordBoxPassword")
                    .Must(BeValidPassword).WithState(account => "PasswordBoxPassword");
        }

        public static bool BeValidPassword(string password)
        {
            bool isValidPassword = false;
            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMiniMaxChars = new Regex(@".{8,15}");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasSymbols = new Regex(@"[@#_]");
            if (hasNumber.IsMatch(password) && hasUpperChar.IsMatch(password) &&
                hasMiniMaxChars.IsMatch(password) && hasLowerChar.IsMatch(password) && hasSymbols.IsMatch(password))
            {
                isValidPassword = true;
            }
            return isValidPassword;
        }
    }
 
}
