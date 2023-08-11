using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace AuthenticationManager.Data.Models
{
    public class Connection
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public IPAddress IpAddress { get; set; }

        [Required]
        public string LastConnectionDateTime { get; set; }

        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }
        
        [Required]
        public User User { get; set; }
    }
}