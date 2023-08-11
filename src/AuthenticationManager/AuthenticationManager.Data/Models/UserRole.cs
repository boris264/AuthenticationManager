using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationManager.Data.Models
{
    public class UserRole
    {
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [Required]
        public User User { get; set; }

        [ForeignKey(nameof(Role))]
        public Guid RoleId { get; set; }

        [Required]
        public Role Role { get; set; }
    }
}