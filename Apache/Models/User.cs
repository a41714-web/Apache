using System;

namespace Apache.Models
{
    /// <summary>
    /// Represents a user account in the Apache system.
    /// Demonstrates inheritance and OOP principles.
    /// </summary>
    public abstract class User
    {
        private string _email;
        private string _password;

        public int Id { get; set; }
        public string Name { get; set; }

        public string Email
        {
            get => _email;
            set
            {
                if (!IsValidEmail(value))
                    throw new ArgumentException("Invalid email format");
                _email = value;
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.Length < 6)
                    throw new ArgumentException("Password must be at least 6 characters");
                _password = value;
            }
        }

        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        protected User()
        {
            CreatedAt = DateTime.Now;
            IsActive = true;
        }

        /// <summary>
        /// Abstract method that must be implemented by derived classes.
        /// </summary>
        public abstract string GetUserRole();

        /// <summary>
        /// Validates email format using simple pattern.
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public override string ToString()
        {
            return $"{Name} ({GetUserRole()}) - {Email}";
        }
    }

    /// <summary>
    /// Represents a regular customer in the Apache system.
    /// </summary>
    public class Customer : User
    {
        public string Address { get; set; }
        public string PhoneNumber { get; set; }

        public override string GetUserRole()
        {
            return "Customer";
        }
    }

    /// <summary>
    /// Represents an administrator in the Apache system.
    /// </summary>
    public class Admin : User
    {
        public string Department { get; set; }

        public override string GetUserRole()
        {
            return "Admin";
        }
    }
}
