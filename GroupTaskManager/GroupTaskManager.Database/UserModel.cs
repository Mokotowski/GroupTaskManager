using Microsoft.AspNetCore.Identity;

namespace GroupTaskManager.GroupTaskManager.Database
{
    public class UserModel : IdentityUser
    {
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
    }
}
