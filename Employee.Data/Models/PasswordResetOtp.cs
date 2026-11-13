//namespace Employee.Data.Models
//{
//    public class PasswordResetOtp
//    {
//        public int Id { get; set; }

//        public int UserId { get; set; }

//        public string OtpHash { get; set; } = string.Empty;

//        public DateTime ExpiryTime { get; set; }

//        public bool IsVerified { get; set; } = false;

//        public bool IsUsed { get; set; } = false;

//        public int Attempts { get; set; } = 0;

//        public DateTime CreatedAt { get; set; }
//    }
//}




namespace Employee.Data.Models
{
    public class PasswordResetOtp
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string OtpHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiryTime { get; set; }

        public bool IsVerified { get; set; }

        public bool IsUsed { get; set; }

        public int Attempts { get; set; }

        public User? User { get; set; }
    }
}

