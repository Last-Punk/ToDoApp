using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Data.Models;

public class User : IdentityUser
{
    [InverseProperty("User")]
    public ICollection<Task>? Tasks { get; set; }
}
