using FluentValidation;
using TrabajosExpres.Models;
using System.Text.RegularExpressions;

namespace TrabajosExpres.Validators
{
    public class MemberATEValidator : AbstractValidator<MemberATE>
    {
        public MemberATEValidator()
        {
            RuleFor(account => account.username).NotEmpty().WithState(account => "TextBoxUserName")
                    .MaximumLength(20).WithState(account => "TextBoxUserName")
                    .MinimumLength(8).WithState(account => "TextBoxUserName")
                    .Matches(@"^([A-Z0-9]{1}[a-z0-9]+[\\s0-9]*)+\$").WithState(account => "TextBoxUserName"); 

            RuleFor(account => account.password).NotEmpty().WithState(account => "PasswordBoxPassword")
                    .MaximumLength(15).WithState(account => "PasswordBoxPassword")
                    .MinimumLength(8).WithState(account => "PasswordBoxPassword")
                    .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[\$@\$!%*?&#])[A-Za-z\\d\$@\$!%*?&#]{8,15}").WithState(account => "PasswordBoxPassword");

            RuleFor(account => account.name).NotEmpty().WithState(account => "TextBoxName")
                    .MaximumLength(150).WithState(account => "TextBoxName")
                    .MinimumLength(3).WithState(account => "TextBoxName")
                    .Matches(@"^([A-ZÁÉÍÓÚ]{1}[a-zñáéíóú]+[\\s]*)+\$").WithState(account => "TextBoxName");

            RuleFor(account => account.lastName).NotEmpty().WithState(account => "TextBoxLastName")
                    .MaximumLength(150).WithState(account => "TextBoxLastName")
                    .MinimumLength(3).WithState(account => "TextBoxLastName")
                    .Matches(@"^([A-ZÁÉÍÓÚ]{1}[a-zñáéíóú]+[\\s]*)+\$").WithState(account => "TextBoxLastName");
            
            RuleFor(account => account.dateBirth).NotEmpty().WithState(account => "DatePickerDateBirth");

            RuleFor(account => account.idCity).NotEmpty().WithState(account => "ComboBoxCity")
                .GreaterThan(0).WithState(account => "ComboBoxCity");

            RuleFor(account => account.email).NotEmpty().WithState(account => "TextBoxEmail")
                .Must(BeValidEmail).WithState(account => "TextBoxEmail");

            RuleFor(account => account.memberATEStatus).NotEmpty()
                .GreaterThan(0).LessThan(3);

            RuleFor(account => account.memberATEType).NotEmpty()
                .GreaterThan(0).LessThan(4);
        }

        public bool BeValidEmail(string email)
        {
            Regex regularExpression = new Regex("^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$");
            if (email == null || email.Length <=4)
            {
                return false;
            }
            bool hasValidFormat = regularExpression.IsMatch(email);
            bool hasValidLength = email.Length <= 255;
            return hasValidFormat && hasValidLength;
        }
    }
}
