using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Models
{
    public partial class User
    {
        public User()
        {
            Task = new HashSet<Task>();
        }

        public new long Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        [NotMapped]
        public string Password { get; set; }
        public string PassworhHash { get; set; }
        public string PasswordSalt { get; set; }
        public bool LockedOut { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }

        public virtual ICollection<Task> Task { get; set; }
    }
}
