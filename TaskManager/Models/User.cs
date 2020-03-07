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
        ///<summary>Record Id</summary>
        public long Id { get; set; }
        ///<summary>User's fistname</summary>
        public string Firstname { get; set; }
        ///<summary>User's lastname</summary>
        public string Lastname { get; set; }
        ///<summary>User's Email</summary>
        public string Email { get; set; }
        [NotMapped]
        public string Password { get; set; }
        public string PassworhHash { get; set; }
        public string PasswordSalt { get; set; }
        ///<summary>User's Registration Date</summary>
        public DateTime RegistrationDate { get; set; }
        ///<summary>User's Last Update Date</summary>
        public DateTime? LastUpdateDate { get; set; }
        ///<summary>User's Task List</summary>
        public virtual ICollection<Task> Task { get; set; }
    }
}
