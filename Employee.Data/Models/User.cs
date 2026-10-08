namespace Employee.Data.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "Employee";

        public string Department { get; set; } = string.Empty;

        public int? AdminId { get; set; }

        public int? ManagerId { get; set; }

        public ICollection<Employee1> Employees { get; set; }
            = new List<Employee1>();
    }
}
 

