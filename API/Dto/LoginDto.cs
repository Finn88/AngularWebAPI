﻿using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class LoginDto
    {
        [Required]
        public required string UserName { get; set; }
        [Required]
        public required string Password { get; set; }
    }
}
