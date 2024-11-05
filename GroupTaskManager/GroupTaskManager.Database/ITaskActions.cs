namespace GroupTaskManager.GroupTaskManager.Database
{
    public interface ITaskActions
    {
        public Task ChanageTaskState();
        public Task CompleteTask();
    }
}
