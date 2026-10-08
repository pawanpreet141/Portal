using Employee.API.Controllers;
using FluentValidation;

namespace Employee.API.Validators
{
    public class SignupRequestValidator
        : AbstractValidator<AccountController.SignupRequest>
    {
        public SignupRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required.")
                .MaximumLength(100)
                .WithMessage(
                    "Name cannot exceed 100 characters.");


            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage(
                    "Please enter a valid email address.");


            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required.")

                .Must(password =>
                    password.Length >= 8 &&
                    password.Any(char.IsLower) &&
                    password.Any(char.IsUpper) &&
                    password.Any(char.IsDigit) &&
                    password.Any(ch =>
                        !char.IsLetterOrDigit(ch)))
                .WithMessage(
                    "Password must be at least 8 characters long, " +
                    "include at least one uppercase letter, " +
                    "one number, and one special character.");
        }
    }
}

