using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ToDo.DTOs
{
    public class SignUp
    {
        public required string NationalId { get; set; }
        public required string Password { get; set; }
        public required string Title { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        
    }
}