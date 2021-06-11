using FluentValidation;
using TrabajosExpres.Models;
using System.Text.RegularExpressions;

namespace TrabajosExpres.Validators
{
    public class MemberATEValidator : AbstractValidator<MemberATE>
    {
        public MemberATEValidator(string confirmation)
        {
            RuleFor(account => account.username).NotEmpty().WithState(account => "TextBoxUserName")
                    .MaximumLength(20).WithState(account => "TextBoxUserName")
                    .MinimumLength(3).WithState(account => "TextBoxUserName")
                    .Matches("^[a-zA-Z0-9]+$").WithState(account => "TextBoxUserName"); 

            RuleFor(account => account.password).NotEmpty().WithState(account => "PasswordBoxPassword")
                    .MaximumLength(15).WithState(account => "PasswordBoxPassword").WithState(account => "PasswordBoxConfirmPassword")
                    .MinimumLength(8).WithState(account => "PasswordBoxPassword").WithState(account => "PasswordBoxConfirmPassword")
                    .Equal(confirmation).WithState(account => "PasswordBoxPassword").WithState(account => "PasswordBoxConfirmPassword")
                    .Must(BeValidPassword).WithState(account => "PasswordBoxPassword").WithState(account => "PasswordBoxConfirmPassword");

            RuleFor(account => account.name).NotEmpty().WithState(account => "TextBoxName")
                    .MaximumLength(150).WithState(account => "TextBoxName")
                    .MinimumLength(2).WithState(account => "TextBoxName")
                    .Matches("^[a-zA-ZÁÉÍÓÚáéíóú ]+$").WithState(account => "TextBoxName");

            RuleFor(account => account.lastName).NotEmpty().WithState(account => "TextBoxLastName")
                    .MaximumLength(150).WithState(account => "TextBoxLastName")
                    .MinimumLength(2).WithState(account => "TextBoxLastName")
                    .Matches("^[a-zA-ZÁÉÍÓÚáéíóú ]+$").WithState(account => "TextBoxLastName");
            
            RuleFor(account => account.dateBirth).NotEmpty().WithState(account => "DatePickerDateBirth");

            RuleFor(account => account.idCity).NotEmpty().WithState(account => "ComboBoxCity")
                .GreaterThan(0).WithState(account => "ComboBoxCity");

            RuleFor(account => account.email).NotEmpty().WithState(account => "TextBoxEmail")
                .Must(BeValidEmail).WithState(account => "TextBoxEmail");

            RuleFor(account => account.memberATEStatus).NotEmpty()
                .GreaterThan(0);

            RuleFor(account => account.memberATEType).NotEmpty()
                .GreaterThan(0);
        }

        public static bool BeValidEmail(string email)
        {
            Regex regularExpression = new Regex("^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$");
            if (email == null || email.Length <=4)
            {
                return false;
            }
            bool hasValidFormat = regularExpression.IsMatch(email);
            bool hasValidLength = email.Length <= 254;
            return hasValidFormat && hasValidLength;
        }

        public static bool BeValidPassword(string password)
        {
            bool isValidPassword = false;
            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMiniMaxChars = new Regex(@".{8,15}");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasSymbols = new Regex(@"[$@\$!%*?&#]");
            if (hasNumber.IsMatch(password) && hasUpperChar.IsMatch(password) &&
                hasMiniMaxChars.IsMatch(password) && hasLowerChar.IsMatch(password) && hasSymbols.IsMatch(password))
            {
                isValidPassword = true;
            }
            return isValidPassword;
        }
    }
}
