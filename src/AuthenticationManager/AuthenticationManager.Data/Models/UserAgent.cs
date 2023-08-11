using System;
using System.ComponentModel.DataAnnotations;

namespace AuthenticationManager.Data.Models
{
    public class UserAgent
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Value { get; set; }
    }
}