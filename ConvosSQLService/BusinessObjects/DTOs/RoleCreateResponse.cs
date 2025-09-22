using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;

namespace BusinessObjects.DTOs
{
    public class RoleCreateResponse
    {
        public Guid Id { get; set; }

        public Guid ServerId { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool Mentionable { get; set; }

        public int Position { get; set; }

    }
}
