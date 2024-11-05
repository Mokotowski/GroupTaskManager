namespace GroupTaskManager.GroupTaskManager.Models
{
    public class UserTasksProgress
    {
        public int Id { get; set; } //Id_Task
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Id_User { get; set; }
        public string State { get; set; }
        public bool Completed { get; set; }
        public DateTime? CompletedTime { get; set; }
        public bool AddToTask { get; set; }
    }
}
