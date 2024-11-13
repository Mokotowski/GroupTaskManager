using GroupTaskManager.GroupTaskManager.Database;
using GroupTaskManager.GroupTaskManager.Models;

namespace GroupTaskManager.GroupTaskManager.Services.Interface
{
    public interface ITaskManageServices
    {
        public Task CreateTask(UserModel user, int Id_Group, string Name, string Description, string AnswerType, DateTime end);
        public Task UpdateTask(UserModel user, int Id, int Id_Group, string Name, string Description, string AnswerType, DateTime end);
        public Task DeleteTask(UserModel user, int Id, int Id_Group);


        public Task AddTaskUser(UserModel user, int Id_Task, string Id_user);
        public Task DeleteTaskUser(UserModel user, int Id_Task, string Id_user);

        public Task<List<UserTasksProgress>> GetUsersTask(UserModel user, int Id_Task); // osoby mające taska i pracujące nad nim
        public Task<List<UserTasksProgress>> GetUsersGroupForTask(UserModel user, int Id_Task); // zarządanie komu dać taska

        public Task<List<TaskRecord>> MyManageTasks(UserModel user, int Id_Group); // Wgląd w taski na danej grupie
        public Task<TaskRecord> MyManageTask(UserModel user, int Id_Task); // Dane jednego taska


    }
}
