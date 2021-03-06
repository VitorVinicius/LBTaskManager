﻿namespace TaskManager.Models
{
    public partial class Task
    {
        ///<summary>Record List</summary>
        public long Id { get; set; }
        ///<summary>Task Description</summary>
        public string Description { get; set; }
        ///<summary>Task Status Flag</summary>
        public bool Concluded { get; set; }
        ///<summary>Owner user ID</summary>
        public long UserId { get; set; }
        ///<summary>Owner user</summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual User User { get; set; }
    }
}
