using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ToDo.DTOs
{
    public class Login
    {
        public required string NationalId { get; set; }
        public required string Password { get; set; }
        
    }
}