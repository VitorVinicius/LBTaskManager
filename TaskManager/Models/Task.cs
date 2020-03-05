using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TaskManager.Models
{
    public partial class Task
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public bool Concluded { get; set; }
        public long UserId { get; set; }
        [JsonIgnore]
        public virtual User User { get; set; }
    }
}
