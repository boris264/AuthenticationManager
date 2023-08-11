using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AuthenticationManager.Data.Constants;

namespace AuthenticationManager.Data.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(UserConstants.UsernameMaxLength)]
        public string Username { get; set; }

        [Required]
        [StringLength(UserConstants.EmailMaxLength)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public ICollection<UserClaim> UserClaims { get; set; } = new List<UserClaim>();

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public ICollection<Connection> UserConnections { get; set; }  = new List<Connection>();
        
        public ICollection<UserUserAgent> UserUserAgents { get; set; } 
            = new List<UserUserAgent>();
    }
}