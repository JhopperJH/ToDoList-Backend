using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ToDo.DTOs
{
    public class ToDoList
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public DateTime Deadline { get; set; }
    }
}