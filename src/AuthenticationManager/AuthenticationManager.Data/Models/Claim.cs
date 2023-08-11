using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AuthenticationManager.Data.Constants;

namespace AuthenticationManager.Data.Models
{
    public class Claim
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(ClaimConstants.NameMaxLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(ClaimConstants.ValueMaxLength)]
        public string Value { get; set; }
    }
}