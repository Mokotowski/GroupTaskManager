using GroupTaskManager.GroupTaskManager.Database;

namespace GroupTaskManager.GroupTaskManager.Models
{
    public class TaskWorkModel
    {
        public TaskRecord record { get; set; }
        public TaskAnswer answer { get; set; }


        public TaskWorkModel(TaskRecord record, TaskAnswer answer)
        {
            this.record = record;
            this.answer = answer;
        }
    }
}
