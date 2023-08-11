using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationManager.Data.Models
{
    public class UserUserAgent
    {
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [Required]
        public User User { get; set; }

        [ForeignKey(nameof(UserAgent))]
        public Guid UserAgentId { get; set; }

        [Required]
        public UserAgent UserAgent { get; set; }
    }
}