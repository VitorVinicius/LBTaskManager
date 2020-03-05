using System;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace TaskManager.Models
{
    public class LogonResult
    {
        public bool Authenticated { get; set; }
        public string Created { get; set; }
        public string Expiration { get; set; }
        public string AccessToken { get; set; }
        public string Message { get; set; }
        [JsonIgnore]
        internal ClaimsPrincipal Principal { get; set; }

        public LogonResult() { }
        public LogonResult(bool authenticated, string created, string expiration, string accessToken, string message)
        {
            Authenticated = authenticated;
            Created = created;
            Expiration = expiration;
            AccessToken = accessToken;
            Message = message;
        }


        public override bool Equals(object obj)
        {
            return obj is LogonResult other &&
                   Authenticated == other.Authenticated &&
                   Created == other.Created &&
                   Expiration == other.Expiration &&
                   AccessToken == other.AccessToken &&
                   Message == other.Message;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Authenticated, Created, Expiration, AccessToken, Message);
        }
    }
}