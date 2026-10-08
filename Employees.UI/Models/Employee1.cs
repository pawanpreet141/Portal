using System.ComponentModel.DataAnnotations;

namespace Employees.UI.Models
{
    public class Employee1
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Range(18, 60, ErrorMessage = "Age must be between 18 and 60.")]
        public int Age { get; set; }

        public string Department { get; set; } = string.Empty;

        public decimal Salary { get; set; }

        // This connects the employee to the user
        public int UserId { get; set; }

        // Navigation property
        public User? User { get; set; }

    }
}