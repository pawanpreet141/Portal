using Employee.Data.Models;
using FluentValidation;

namespace Employee.API.Validators
{
    public class EmployeeValidator : AbstractValidator<Employee1>
    {
        public EmployeeValidator()
        {

            RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Employee name is required.")
            .MaximumLength(100)
            .WithMessage(
            "Employee name cannot exceed 100 characters.");


        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage(
                "Please enter a valid email address.");


            RuleFor(x => x.Age)
                .InclusiveBetween(18, 60)
                .WithMessage(
                    "Age must be between 18 and 60.");


            RuleFor(x => x.Team)
                .NotEmpty()
                .WithMessage("Employee team is required.")
                .Must(IsValidTeam)
                .WithMessage(
                    "Team must be Designer, Development, Sales, SEO, " +
                    "System Engineer, or DevOps.");


            RuleFor(x => x.Salary)
                .GreaterThan(0)
                .WithMessage(
                    "Salary must be greater than 0.");
        }


        private bool IsValidTeam(string team)
        {
            string[] validTeams =
            {
            "Designer",
            "Development",
            "Sales",
            "SEO",
            "System Engineer",
            "DevOps"
        };

            return validTeams.Contains(
                team.Trim(),
                StringComparer.OrdinalIgnoreCase);
        }
    }

}

