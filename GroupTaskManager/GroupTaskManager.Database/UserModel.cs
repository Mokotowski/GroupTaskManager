using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GroupTaskManager.GroupTaskManager.Database
{
    public class UserModel : IdentityUser
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string? Firstname { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string? Lastname { get; set; }

        // Initialize the list to avoid null reference exceptions
        public List<Group> Group { get; set; } = new List<Group>();
    }
}