using System;

namespace Apache.Models
{
    /// <summary>
    /// Classe Interface que define um utilizador
    /// </summary>
    public interface IUser
    {
        int Id { get; set; }
        string? Name { get; set; }
        string Email { get; set; }
        string Password { get; set; }
        DateTime CreatedAt { get; set; }
        bool IsActive { get; set; }
        string GetUserRole();
    }

    /// <summary>
    /// Classe Abstrata que representa um utilizador
    /// </summary>
    public abstract class User : IUser
    {
        private string _email = string.Empty;
        private string _password = string.Empty;

        public int Id { get; set; }
        public string? Name { get; set; }

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
        /// Método abstrato para obter o papel do utilizador
        /// </summary>
        public abstract string GetUserRole();

        /// <summary>
        /// Validação simples do formato de email.
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                // System.Net.Mail.MailAddress faz a validação básica do formato do email
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
    /// Classe de um utilizador cliente
    /// </summary>
    public class Customer : User
    {
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }

        public override string GetUserRole()
        {
            return "Customer";
        }
    }

    /// <summary>
    /// Classe de um utilizador administrador
    /// </summary>
    public class Admin : User
    {
        public string? Department { get; set; }

        public override string GetUserRole()
        {
            return "Admin";
        }
    }
}
