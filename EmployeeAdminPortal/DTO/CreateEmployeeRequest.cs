namespace Employee.API.DTOs
{
    public class CreateEmployeeRequest
    {
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int Age { get; set; }

        public decimal Salary { get; set; }

        public string Team { get; set; } = string.Empty;

    }
}