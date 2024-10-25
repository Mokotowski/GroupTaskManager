using System.ComponentModel.DataAnnotations;

namespace GroupTaskManager.GroupTaskManager.Database
{
    public class Group_User
    {
        [Key]
        public int Id { get; set; }
        public string Id_User { get; set; }
        public int Id_Group { get; set; }

        public Group Group { get; set; }
    }
}
