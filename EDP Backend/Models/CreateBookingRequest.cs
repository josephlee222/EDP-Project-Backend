﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDP_Backend.Models
{
    public class CreateBookingRequest
    {
        public int UserId { get; set; }
        public int ActivityId { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public int Pax {  get; set; }
        public string? Notes { get; set; }
    }
}
