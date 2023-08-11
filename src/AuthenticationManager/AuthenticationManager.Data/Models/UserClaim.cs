using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationManager.Data.Models
{
    public class UserClaim
    {
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [Required]
        public User User { get; set; }

        [ForeignKey(nameof(Claim))]
        public Guid ClaimId { get; set; }

        [Required]
        public Claim Claim { get; set; }
    }
}