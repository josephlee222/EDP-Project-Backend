﻿using System.ComponentModel.DataAnnotations;

namespace EDP_Backend.Models
{
    public class Group
    {
        public int Id { get; set; }
        [MaxLength(32)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(64)]
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
