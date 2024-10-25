using System.ComponentModel.DataAnnotations;

namespace GroupTaskManager.GroupTaskManager.Database
{
    public class Group
    {
        [Key]
        public int Id { get; set; }
        public string Id_User { get; set; }

        public UserModel User { get; set; }
        public List<Group_User> Group_User { get; set; }
        public List<TaskRecord> TaskRecord { get; set; }
    }
}
