using GroupTaskManager.GroupTaskManager.Database;

namespace GroupTaskManager.GroupTaskManager.Models
{
    public class SingleTaskData
    {
        public TaskRecord TaskData { get; set; }
        public List<UserTasksProgress> Users_task { get; set; }
        public List<UserTasksProgress> Users_group { get; set; }

    }
}



