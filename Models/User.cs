using System;
using System.Collections.Generic;

namespace ToDo.Models;

public partial class User
{
    public int Id { get; set; }

    public string NationalId { get; set; } = null!;

    public string Salt { get; set; } = null!;

    public string HashedPassword { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public virtual ICollection<Activity> Activity { get; set; } = new List<Activity>();
}
