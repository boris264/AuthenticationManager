using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AuthenticationManager.Data.Constants;

namespace AuthenticationManager.Data.Models
{
    public class Role
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(RoleConstants.NameMaxLength)]
        public string Name { get; set; }
    }
}